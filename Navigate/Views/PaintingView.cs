using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Timers;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;

namespace Mirea.Snar2017.Navigate
{
    class PaintingView : AndroidGameView
    {
        private (float X, float Y) previousPositionTouch;
        private (float Z, float XY) angle;
        private (float X, float Y, float Z) position;
        private (int Width, int Height) viewport;
        private float speedMultiplier = 1;

        private List<float> trace = new List<float>();
        private float[] stateVector = new float[8];

        private ScaleGestureDetector scaleDetector;
        private static float scaleFactor = 1.0f;

        private Timer stateVectorChangeTimer = new Timer();

        #region Constructors
        public PaintingView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public PaintingView(IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
            Initialize();
        }

        private void Initialize()
        {
            scaleDetector = new ScaleGestureDetector(Context, new ScaleListener());
            stateVectorChangeTimer.AutoReset = false;
            angle.Z = 0;
            angle.XY = 0;
        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ShadeModel(All.Flat);
            GL.Enable(All.CullFace);
            GL.ClearDepth(1.0f);
            GL.Enable(All.DepthTest);
            GL.DepthFunc(All.Lequal);
            GL.Enable(All.CullFace);
            GL.CullFace(All.Back);
            GL.Hint(All.PerspectiveCorrectionHint, All.Nicest);

            stateVectorChangeTimer.Elapsed += UpdateCoordinates;
            UpdateCoordinates(null, null);

            RenderFrame += delegate
            {
                SetupCamera();
                RenderCube();
            };

            Run(60);
        }

        private void UpdateCoordinates(object sender, ElapsedEventArgs e)
        {
            if (Storage.CurrentFrame == Storage.NumberOfFrames)
            {
                stateVectorChangeTimer.Elapsed -= UpdateCoordinates;
                return;
            }
            stateVector = Storage.Data[Storage.CurrentFrame];

            stateVectorChangeTimer.Interval = stateVector[0] / speedMultiplier;
            stateVectorChangeTimer.Enabled = true;

            stateVector[1] = 2 * (float)Math.Acos(stateVector[1]);
            stateVector[2] /= (float)Math.Sin(stateVector[1] / 2);
            stateVector[3] /= (float)Math.Sin(stateVector[1] / 2);
            stateVector[4] /= (float)Math.Sin(stateVector[1] / 2);

            Storage.CurrentFrame++;
        }

        public void SetupCamera()
        {
            viewport.Width = Width;
            viewport.Height = Height;

            GL.Viewport(0, 0, viewport.Width, viewport.Height);

            GL.MatrixMode(All.Projection);
            Matrix4 M = Matrix4.CreatePerspectiveFieldOfView(ToRadians(110), (float)viewport.Width / (float)viewport.Height, 0.001f, 10000.0f);
            GL.LoadIdentity();
            GL.LoadMatrix(ConvertMatrix4(M));

            GL.MatrixMode(All.Modelview);
            //Matrix4 m = Matrix4.LookAt(0, 0, 0, -1, -1, -1, 0, 1, 0);
            Matrix4 m = Matrix4.LookAt(1, 1, 1, 0, 0, 0, 0, 1, 0);
            GL.LoadIdentity();
            GL.LoadMatrix(ConvertMatrix4(m));

            GL.Scale(0.1f, 0.1f, 0.1f);
            GL.Rotate(-angle.XY / 10, 0, 1, 0);
            GL.Rotate(-angle.Z / 10, 1, 0, -1);
            GL.Scale(scaleFactor, scaleFactor, scaleFactor);
        }

        public void RenderCube()
        {
            GL.PushMatrix();

            position.X = stateVector[5];
            position.Y = stateVector[7];
            position.Z = stateVector[6];

            GL.Translate(position.X, position.Y, position.Z);
            GL.Rotate(stateVector[1], stateVector[2] + position.X, stateVector[4] + position.Y, stateVector[3] + position.Z);
            trace.Add(position.X);
            trace.Add(position.Y);
            trace.Add(position.Z);

            float[] tr = new float[trace.Count];
            for (int j = 0; j < trace.Count; j++)
                tr[j] = trace[j];

            GL.ClearColor(0, 0.7f, 0.5f, 0.3f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            unsafe
            {
                fixed (float* pcube = cube, pcubeColors = cubeColors)
                {
                    fixed (byte* ptriangles = triangles)
                    {
                        GL.VertexPointer(3, All.Float, 0, new IntPtr(pcube));
                        GL.EnableClientState(All.VertexArray);
                        GL.ColorPointer(4, All.Float, 0, new IntPtr(pcubeColors));
                        GL.EnableClientState(All.ColorArray);
                        GL.DrawElements(All.Triangles, 36, All.UnsignedByte, new IntPtr(ptriangles));
                    }
                }
                GL.PopMatrix();
                fixed (float* pline = line, plineColor = lineColor)
                {
                    GL.VertexPointer(3, All.Float, 0, new IntPtr(pline));
                    GL.EnableClientState(All.VertexArray);
                    GL.ColorPointer(4, All.Float, 0, new IntPtr(plineColor));
                    GL.EnableClientState(All.ColorArray);
                    GL.DrawArrays(All.Lines, 0, 6);
                }

                fixed (float* pline = tr)
                {
                    GL.VertexPointer(3, All.Float, 0, new IntPtr(pline));
                    GL.EnableClientState(All.VertexArray);
                    GL.DrawArrays(All.LineStrip, 0, trace.Count / 3);
                }

                SwapBuffers();
            }
        }

        #region View handlers
        public override bool OnTouchEvent(MotionEvent e)
        {
            scaleDetector.OnTouchEvent(e);

            if (e.Action == MotionEventActions.Down)
            {
                previousPositionTouch.X = e.GetX();
                previousPositionTouch.Y = e.GetY();
            }
            if (e.Action == MotionEventActions.Move)
            {
                if (!scaleDetector.IsInProgress)
                {
                    float e_x = e.GetX();
                    float e_y = e.GetY();

                    float xDiff = (previousPositionTouch.X - e_x);
                    float yDiff = (previousPositionTouch.Y - e_y);
                    angle.Z = angle.Z + yDiff;
                    angle.XY = angle.XY + xDiff;
                    previousPositionTouch.X = e_x;
                    previousPositionTouch.Y = e_y;
                }
            }
            if (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Move)
                RenderCube();
            return true;
        }
        #endregion

        private static float[] ConvertMatrix4(Matrix4 M)
        {
            float[] Array = new float[16];
            Array[0] = M.Row0.X;
            Array[1] = M.Row0.Y;
            Array[2] = M.Row0.Z;
            Array[3] = M.Row0.W;
            Array[4] = M.Row1.X;
            Array[5] = M.Row1.Y;
            Array[6] = M.Row1.Z;
            Array[7] = M.Row1.W;
            Array[8] = M.Row2.X;
            Array[9] = M.Row2.Y;
            Array[10] = M.Row2.Z;
            Array[11] = M.Row2.W;
            Array[12] = M.Row3.X;
            Array[13] = M.Row3.Y;
            Array[14] = M.Row3.Z;
            Array[15] = M.Row3.W;
            return Array;
        }

        private static float ToRadians(float degrees)
        {
            return (float)(degrees * (System.Math.PI / 180.0));
        }

        #region Arrays for OpenGl
        float[] cube = {
            -3f, 0.2f, 1f, // vertex[0]
			3f, 0.2f, 1f, // vertex[1]
			3f, -0.2f, 1f, // vertex[2]
			-3f, -0.2f, 1f, // vertex[3]
			-3f, 0.2f, -1f, // vertex[4]
			3f, 0.2f, -1f, // vertex[5]
			3f, -0.2f, -1f, // vertex[6]
			-3f, -0.2f, -1f, // vertex[7]
		};
        float[] line = {
            0, 10000, 0,
            0, -10000, 0,
            10000, 0, 0,
            -10000, 0, 0,
            0, 0, 10000,
            0, 0, -10000,
        };
        float[] lineColor = {
            0, 0, 1, 0,
            0, 0, 1, 0,
            1, 0, 0, 0,
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 1, 0, 0,
            };
        byte[] triangles = {
            1, 0, 2, // front
			3, 2, 0,
            6, 4, 5, // back
			4, 6, 7,
            4, 7, 0, // left
			7, 3, 0,
            1, 2, 5, //right
			2, 6, 5,
            0, 1, 5, // top
			0, 5, 4,
            2, 3, 6, // bottom
			3, 7, 6,
        };
        float[] cubeColors = {
            1.0f, 0.0f, 0.0f, 0.0f,
            1.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0f,
            0.0f, 1.0f, 0.0f, 0f,
        };
        #endregion

        /*private void PrintText3D(double x, double y, double z, string text)
        {
            GL.DrawText();
            Glut.glutBitmapString(Glut.GLUT_BITMAP_9_BY_15, text);
        }*/
        private class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            public override bool OnScale(ScaleGestureDetector detector)
            {
                scaleFactor *= detector.ScaleFactor;
                return true;
            }
        }
    }
}