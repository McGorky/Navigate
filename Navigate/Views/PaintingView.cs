using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

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
        private Vector3 position;
        private Vector3 previousPositionTouch;
        private (float Z, float XY) angle;
        private (float Z, float XY) realAngle;
        private (float Z, float XY) previousAngle;

        Quaternion axisXY = new Quaternion();
        Quaternion axisZ = new Quaternion();
        Quaternion vector = new Quaternion();
        private float offset = 1f; 
        private float teta;
        private float lengthCameraVector;
        static float x = 0.001f;
        //private Vector3 pop;

        private (int Width, int Height) viewport;
        private int activePointerId = -1;

        private List<float> trace = new List<float>();
        private float[] stateVector = new float[8];
        private float[] pop = new float[3] {-1, -1, -1 };
        private float[] pop1 = new float[3] {1, 1, 1};
        private float[] off = new float[3];

        private ScaleGestureDetector scaleDetector;
        private static float scaleFactor = 1.0f;

        private Timer starter;

        public float SpeedMultiplier { get; set; } = 1.0f;
        public bool DrawTrajectory { get; set; } = true;
        public bool IsPlaying { get; set; } = false;
        public bool freeCamera { get; set; } = true;
        public event Action CoordinatesUpdated;
        public event Action Finished;

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
            //stateVectorChangeTimer.AutoReset = false;
            angle.Z = 0;
            angle.XY = 0;
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            Storage.CurrentFrame = 0;
            base.OnClosed(e);
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

            Storage.CurrentFrame = 0;
            starter = new Timer(UpdateCoordinates);
            UpdateCoordinates(null);

            RenderFrame += delegate
            {
                SetupCamera();
                RenderCube();
            };

            Run(60);
        }

        private void UpdateCoordinates(object sender)
        {
            if (!IsPlaying)
            {
                starter.Change((int)(Math.Round(stateVector[0] / SpeedMultiplier, MidpointRounding.AwayFromZero)), Timeout.Infinite);
                return;
            }

            Storage.CurrentFrame++;
            if (Storage.CurrentFrame == Storage.NumberOfFrames)
            {
                //Toast.MakeText(Context, "finished", ToastLength.Long).Show();
                Finished();
                starter.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }
            CoordinatesUpdated();
            stateVector = Storage.Data[Storage.CurrentFrame];

            stateVector[1] = 2 * (float)(Math.Acos(stateVector[1]) * 180 / Math.PI);
            stateVector[2] /= Math.Abs((float)Math.Sin(stateVector[1] / 2));
            stateVector[3] /= Math.Abs((float)Math.Sin(stateVector[1] / 2));
            stateVector[4] /= Math.Abs((float)Math.Sin(stateVector[1] / 2));
            
            if (DrawTrajectory)
            {
                position.X = stateVector[5];
                position.Y = stateVector[7];
                position.Z = stateVector[6];
            }

            starter.Change((int)(Math.Round(stateVector[0] / SpeedMultiplier, MidpointRounding.AwayFromZero)), Timeout.Infinite);
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

            if (freeCamera)
            {
                #region freeCamera

                GL.MatrixMode(All.Modelview);

                if (previousAngle.Z != angle.Z || previousAngle.XY != angle.XY)
                {
                    realAngle.Z = angle.Z - previousAngle.Z;
                    realAngle.XY = angle.XY - previousAngle.XY;

                    lengthCameraVector = (float)Math.Sqrt(pop[0] * pop[0] + pop[1] * pop[1] + pop[2] * pop[2]);
                    teta = (float)Math.Acos(Math.Abs(pop[1]) / lengthCameraVector);
                    teta = (float)Math.PI / 2 - teta;
                    axisZ[1] = (float)Math.Cos(teta / 2);
                    axisZ[2] = -(float)Math.Sin(teta / 2);
                    axisZ[3] = 0;
                    axisZ[4] = (float)Math.Sin(teta / 2);

                    vector[1] = 0;
                    vector[2] = 0;
                    vector[3] = 1;
                    vector[4] = 0;

                    var Z = axisZ * vector;
                    Z.Normalized();
                    vector = Z * axisZ.Inversed();
                    vector.Normalized();
                    //
                    axisZ[1] = (float)Math.Cos(ToRadians(realAngle.Z / 20));
                    axisZ[2] = vector[2] * (float)Math.Sin(ToRadians(realAngle.Z / 20));
                    axisZ[3] = vector[3] * (float)Math.Sin(ToRadians(realAngle.Z / 20));
                    axisZ[4] = vector[4] * (float)Math.Sin(ToRadians(realAngle.Z / 20));

                    axisXY[1] = (float)Math.Cos(ToRadians(realAngle.XY / 20));
                    axisXY[2] = (float)Math.Sin(ToRadians(realAngle.XY / 20));
                    axisXY[3] = 0;
                    axisXY[4] = -(float)Math.Sin(ToRadians(realAngle.XY / 20));

                    vector[1] = 0;
                    vector[2] = pop[0];
                    vector[3] = pop[1];
                    vector[4] = pop[2];

                    var Q = axisZ * axisXY;
                    Q = Q.Normalized();
                    var T = Q * vector;
                    T = T.Normalized();
                    vector = T * Q.Inversed();
                    vector = vector.Normalized();
                    pop[0] = vector[2];
                    pop[1] = vector[3];
                    pop[2] = vector[4];
                }
                /*off[0] = 200f;
                off[1] = 200f;
                off[2] = 200f;*/
                if (scaleFactor < offset)
                    for (int i = 0; i < 3; i++) off[i] -= pop[i] / 10;
                if (scaleFactor > offset)
                    for (int i = 0; i < 3; i++) off[i] += pop[i] / 10;

                Matrix4 m = Matrix4.LookAt(0.5f + off[0], 0.5f + off[1], 0.5f + off[2], 0.5f + off[0] + pop[0], 0.5f + off[1] + pop[1], 0.5f + off[2] + pop[2], 0, 1, 0);
                GL.LoadMatrix(ConvertMatrix4(m));

                previousAngle.Z = angle.Z;
                previousAngle.XY = angle.XY;
                offset = scaleFactor;

                #endregion
            }
            else
            {
                #region слежка за объектом
                /*
                float offset = -0.9f + 1 / scaleFactor;
                GL.MatrixMode(All.Modelview);
                if (offset < 0) scaleFactor = 1 / 0.9f;
                offset = -0.9f + 1 / scaleFactor;
                Matrix4 m = Matrix4.LookAt(position.X + offset, position.Y + offset, position.Z + offset, position.X, position.Y, position.Z, 0, 1, 0);
                GL.LoadMatrix(ConvertMatrix4(m));

                GL.Translate(position.X, position.Y, position.Z);
                GL.Rotate(-angle.XY / 10, 1, 0, -1);
                GL.Rotate(-angle.Z / 10, 0, 1, 0);
                GL.Translate(-position.X, -position.Y, -position.Z);
                */
                #endregion

                #region centerCamera
                /*
                GL.MatrixMode(All.Modelview);
                Matrix4 m = Matrix4.LookAt(0.1f, 0.1f, 0.1f, 0, 0, 0, 0, 1, 0);
                GL.LoadIdentity();
                GL.LoadMatrix(ConvertMatrix4(m));

                //GL.Scale(10f, 10f, 10f);
                GL.Rotate(-angle.XY / 10, 1, 0, -1);
                GL.Rotate(-angle.Z / 10, 0, 1, 0);
                GL.Scale(scaleFactor, scaleFactor, scaleFactor);
                */
                #endregion
            }
        }

        public void RenderCube()
        {
            GL.PushMatrix();

            GL.Translate(position.X, position.Y, position.Z);
            //GL.Rotate(stateVector[1], stateVector[2] + position.X, stateVector[4] + position.Y, stateVector[3] + position.Z);
            GL.Rotate(stateVector[1], stateVector[2], stateVector[4], stateVector[3]);
            trace.Add(position.X);
            trace.Add(position.Y);
            trace.Add(position.Z);

            float[] tr = new float[trace.Count];
            for (int j = 0; j < trace.Count; j++)
                tr[j] = trace[j];

            GL.ClearColor(1, 1, 1, 1f);
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

                GL.PushMatrix();
                float[] modelview = new float[16];

                GL.GetFloat(All.ModelviewMatrix, modelview);
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        if (i == j)
                            modelview[i * 4 + j] = 1.0f;
                        else
                            modelview[i * 4 + j] = 0.0f;
                    }
                GL.LoadMatrix(modelview);

                fixed (float* pbillboard = billboard, pcubeColors = cubeColors)
                {
                    fixed (byte* btriangles = billtriangles)
                    {
                        GL.VertexPointer(3, All.Float, 0, new IntPtr(pbillboard));
                        GL.EnableClientState(All.VertexArray);
                        GL.ColorPointer(4, All.Float, 0, new IntPtr(pcubeColors));
                        GL.EnableClientState(All.ColorArray);
                        GL.DrawElements(All.Triangles, 6, All.UnsignedByte, new IntPtr(btriangles));
                    }
                }
                GL.PopMatrix();

                SwapBuffers();
            }
        }

        #region View handlers
        public override bool OnTouchEvent(MotionEvent e)
        {
            scaleDetector.OnTouchEvent(e);
            int pointerIndex;

            switch (e.Action & MotionEventActions.Mask)
            {
                case MotionEventActions.Down:
                previousPositionTouch.X = e.GetX();
                previousPositionTouch.Y = e.GetY();
                activePointerId = e.GetPointerId(0);
                break;

                case MotionEventActions.Move:
                pointerIndex = e.FindPointerIndex(activePointerId);
                float e_x = e.GetX();
                float e_y = e.GetY();
                if (!scaleDetector.IsInProgress)
                {

                    float xDiff = (previousPositionTouch.X - e_x);
                    float yDiff = (previousPositionTouch.Y - e_y);
                    angle.XY += yDiff;
                    if (!freeCamera)
                    {
                        if (angle.XY > 3600) angle.XY -= 3600;
                        if (angle.XY < -3600) angle.XY += 3600;
                        if ((angle.XY > 1100 && angle.XY < 2900) || (angle.XY < -1100 && angle.XY > -2900))
                            angle.Z -= xDiff;
                        else
                            angle.Z += xDiff;                           
                    }
                    else angle.Z += xDiff; ;
                    }
                previousPositionTouch.X = e_x;
                previousPositionTouch.Y = e_y;
                break;

                case MotionEventActions.Cancel:
                activePointerId = -1;
                break;

                case MotionEventActions.PointerUp:
                pointerIndex = (int)(e.Action & MotionEventActions.PointerIndexMask) >> (int)MotionEventActions.PointerIndexShift;
                int pointerId = e.GetPointerId(pointerIndex);
                if (pointerId == activePointerId)
                {
                    int newPointerIndex = pointerIndex == 0 ? 1 : 0;
                    previousPositionTouch.X = e.GetX(newPointerIndex);
                    previousPositionTouch.Y = e.GetY(newPointerIndex);
                    activePointerId = e.GetPointerId(newPointerIndex);
                }
                break;
            }
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
            -0.03f, 0.002f, 0.01f, // vertex[0]
			0.03f, 0.002f, 0.01f, // vertex[1]
			0.03f, -0.002f,0.01f, // vertex[2]
			-0.03f, -0.002f, 0.01f, // vertex[3]
			-0.03f, 0.002f, -0.01f, // vertex[4]
			0.03f, 0.002f, -0.01f, // vertex[5]
			0.03f, -0.002f, -0.01f, // vertex[6]
			-0.03f, -0.002f, -0.01f, // vertex[7]
		};
        float[] line = {
            0, 100, 0,
            0, -100, 0,
            100, 0, 0,
            -100, 0, 0,
            0, 0, 100,
            0, 0, -100,
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
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
        };
        float[] billboard = {
            0.06f + x, -2*x, 0,
            0.06f + x, 2*x, 0,
            0.06f - x, 2*x, 0,
            0.06f - x, -2*x, 0,
        };
        byte[] billtriangles = {
            0, 1, 2,
            3, 0, 2,
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