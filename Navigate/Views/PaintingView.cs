using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

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
        // REMARK ZH: привести в порядок имена (пкм - переименовать)
        float xPrevious, yPrevious;
        float xAngle, yAngle;

        private ScaleGestureDetector scaleDetector;
        private static float scaleFactor = 1.0f;

        List<float> trace = new List<float>();
        float x, y, z;
        float koef;

        // REMARK ZH: сделать нормальный коэффициент скорости
        bool speedUp = false;
        bool first = true;
        float[] stateVector = new float[8];
        int viewportWidth, viewportHeight;

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
            xAngle = 0;
            yAngle = 0;
        }
        #endregion

        protected override void CreateFrameBuffer()
        {
            ContextRenderingApi = GLVersion.ES1;
            try
            {
                // REMARK ZH: зачем тут Log?
                Log.Verbose("GLCube", "Loading with default settings");
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("GLCube", "{0}", ex);
            }

            try
            {
                Log.Verbose("GLCube", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 4, 0, 0, 0, false);
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("GLCube", "{0}", ex);
            }
            throw new Exception("Can't load egl, aborting");
        }
        
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


            EventHandler<FrameEventArgs> updateCoordinates = null;
            updateCoordinates = delegate
            {
                if (Storage.CurrentFrame == Storage.NumberOfFrames)
                {
                    UpdateFrame -= updateCoordinates;
                    return;
                }
                stateVector = Storage.Data[Storage.CurrentFrame];
                Storage.CurrentFrame++;

                if (first)
                {
                    koef = 1000000 / stateVector[0] / 60;
                    first = false;
                }

                var angle = Math.Acos(stateVector[1]) * 2;
                if (speedUp == false)
                {
                    stateVector[1] = (float)angle * koef;
                    stateVector[5] *= koef;
                    stateVector[6] *= koef;
                    stateVector[7] *= koef;
                }
                else
                {
                    stateVector[1] = 2 * (float)angle * koef;
                    stateVector[5] *= 2 * koef;
                    stateVector[6] *= 2 * koef;
                    stateVector[7] *= 2 * koef;
                }
            };
            UpdateFrame += updateCoordinates;

            RenderFrame += delegate
            {
                SetupCamera();
                RenderCube();
            };

            Run(60);
        }

        void SetupCamera()
        {
            viewportWidth = Width;
            viewportHeight = Height;

            GL.Viewport(0, 0, viewportWidth, viewportWidth);

            Matrix4 M = Matrix4.CreatePerspectiveFieldOfView(ToRadians(90), (float)viewportWidth / (float)viewportHeight, 0.001f, 1000.0f);

            float[] perspective_m1 = new float[16];
            int i = 0;

            // REMARK ZH: никогда не пиши несколько команд на одной строке
            // REMARK ZH: и нужно отрефакторить все
            perspective_m1[i + 0] = M.Row0.X;perspective_m1[i + 1] = M.Row0.Y;
            perspective_m1[i + 2] = M.Row0.Z; perspective_m1[i + 3] = M.Row0.W;
            i += 4;

            perspective_m1[i + 0] = M.Row1.X; perspective_m1[i + 1] = M.Row1.Y;
            perspective_m1[i + 2] = M.Row1.Z; perspective_m1[i + 3] = M.Row1.W;
            i += 4;

            perspective_m1[i + 0] = M.Row2.X; perspective_m1[i + 1] = M.Row2.Y;
            perspective_m1[i + 2] = M.Row2.Z; perspective_m1[i + 3] = M.Row2.W;
            i += 4;

            perspective_m1[i + 0] = M.Row3.X; perspective_m1[i + 1] = M.Row3.Y;
            perspective_m1[i + 2] = M.Row3.Z; perspective_m1[i + 3] = M.Row3.W;
            GL.LoadMatrix(perspective_m1);

            GL.MatrixMode(All.Modelview);
            Matrix4 m = Matrix4.LookAt(0, 0, 0, -1, -1, -1, 0, 1, 0);
            float[] perspective_m = new float[16];

            i = 0;
            perspective_m[i + 0] = m.Row0.X; perspective_m[i + 1] = m.Row0.Y;
            perspective_m[i + 2] = m.Row0.Z; perspective_m[i + 3] = m.Row0.W;
            i += 4;

            perspective_m[i + 0] = m.Row1.X; perspective_m[i + 1] = m.Row1.Y;
            perspective_m[i + 2] = m.Row1.Z; perspective_m[i + 3] = m.Row1.W;
            i += 4;

            perspective_m[i + 0] = m.Row2.X; perspective_m[i + 1] = m.Row2.Y;
            perspective_m[i + 2] = m.Row2.Z; perspective_m[i + 3] = m.Row2.W;
            i += 4;

            perspective_m[i + 0] = m.Row3.X; perspective_m[i + 1] = m.Row3.Y;
            perspective_m[i + 2] = m.Row3.Z; perspective_m[i + 3] = m.Row3.W;
            GL.LoadIdentity();
            GL.LoadMatrix(perspective_m);
            GL.Scale(0.1f, 0.1f, 0.1f);
            GL.Rotate(-yAngle / 10, 0, 1, 0);
            GL.Rotate(-xAngle / 10, 1, 0, -1);
            GL.Scale(scaleFactor, scaleFactor, scaleFactor);
        }

        void RenderCube()
        {
            GL.PushMatrix();

            x = stateVector[5];
            y = stateVector[7];
            z = stateVector[6];

            GL.Translate(x, y, z);
            GL.Rotate(stateVector[1], stateVector[2] + x, stateVector[4] + y, stateVector[3] + z);
            trace.Add(x);
            trace.Add(y);
            trace.Add(z);

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

        private static float ToRadians(float degrees)
        {
            return (float)(degrees * (System.Math.PI / 180.0));
        }

        #region View handlers
        public override bool OnTouchEvent(MotionEvent e)
        {
            scaleDetector.OnTouchEvent(e);

            if (e.Action == MotionEventActions.Down)
            {
                xPrevious = e.GetX();
                yPrevious = e.GetY();
            }
            if (e.Action == MotionEventActions.Move)
            {
                if (!scaleDetector.IsInProgress)
                {
                    float e_x = e.GetX();
                    float e_y = e.GetY();

                    float xdiff = (xPrevious - e_x);
                    float ydiff = (yPrevious - e_y);
                    xAngle = xAngle + ydiff;
                    yAngle = yAngle + xdiff;
                    xPrevious = e_x;
                    yPrevious = e_y;
                }
            }
            if (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Move)
                RenderCube();
            return true;
        }
        #endregion

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
            0, 500, 0,
            0, -500, 0,
            500, 0, 0,
            -500, 0, 0,
            0, 0, 500,
            0, 0, -500,
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