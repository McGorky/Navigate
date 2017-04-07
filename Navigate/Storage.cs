using System;
using System.IO;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Mirea.Snar2017.Navigate
{
    // REMARK ALL: ��� ������ ��������� �������� � ������ �������� � ������� ����
    // REMARK ALL: ����������� #region - #endregion ��� ����, ����� ������������� ��� (�� ������������ ������ �������)
    // REMARK ALL: ������� ���� ���������� ���������� ����� �����, ����� ����� �� ����������� ���������� ������������� https://msdn.microsoft.com/ru-ru/library/ms229002(v=vs.110).aspx
    // REMARK ALL: �� ������������� ��� ������������

    // CONSIDER: �������������
    public class Storage
    {
        public static class ForegroundServiceId
        {
            public static int SenorsData = 812563123;
        }

        public static object DataAccessSync { get; } = new object();

        #region LogPlayer data
        // UNDONE: �����������
        public static int CurrentFrame = 0;
        public static int NumberOfFrames = 0;
        public static float[][] Data = null;
        #endregion

        #region Raw data
        // CONSIDER: �����������, ����� �� ��� set
        public static float[] AccelerometerData { get; } = new float[3];
        public static float[] GyroscopeData { get; } = new float[3];
        public static float[] MagnetometerData { get; } = new float[3];
        // TODO: ������������ ������ ������ ������������� ��� ����������� �����������
        public static float[] LinearAccelerationData { get; } = new float[3];

        //public static (float X, float Y, float Z) AccelerometerData { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) GyroscopeData { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) MagnetometerData { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) LinearAccelerationData { get; } = (0, 0, 0);
        #endregion

        #region Filtered data
        public static Quaternion Rotation { get; set; } = (1, 0, 0, 0);
        public static (float Psi, float Theta, float Phi) EulerAngles
        {
            get
            {
                var q = Rotation;
                var psi = (float)Math.Atan2(2 * q[2] * q[3] - 2 * q[1] * q[4], 2 * q[1] * q[1] + 2 * q[2] * q[2] - 1);
                var theta = (float)-Math.Asin(2 * q[2] * q[4] + 2 * q[1] * q[3]);
                var phi = (float)Math.Atan2(2 * q[3] * q[4] - 2 * q[1] * q[2], 2 * q[1] * q[1] + 2 * q[4] * q[4] - 1);

                return (psi, theta, phi);
            }
        }

        public static float[] Acceleration { get; } = new float[3];
        public static float[] Velocity { get; } = new float[3];
        public static float[] Offset { get; } = new float[3];

        //public static (float X, float Y, float Z) Acceleration { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) Velocity { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) Offset { get; } = (0, 0, 0);
        #endregion

        #region Filter parameters
        public static Matrix AccelerometerCalibrationMatrix { get; set; } = new Matrix(4, 4, MatrixInitializationValue.Identity);

        public static Quaternion StartRotation { get; set; } = (1, 0, 0, 0);

        public static float Beta { get; set; } = 0.1f;
        public static float Zeta { get; set; } = 0.1f;
        public static float Gamma { get; set; } = 0.7f;

        public static bool MagnetometerEnabled { get; set; }
        public static bool GyroscopeDriftCompensationEnabled { get; set; }
        public static bool AccelerometerCalibrationEnabled { get; set; }
        public static bool TrajectoryTracingEnabled { get; set; }

        public static TimeSpan Uptime { get; set; } = new TimeSpan();
        public static DateTime StartTime { get; set; }
        #endregion

        #region File parameters
        public static string Path { get; set; } = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);//(string)Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);//(string)Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments); 
        // TODO: �������� �� ������ ��������� ������ ����
        public static string Filename { get; set; } = System.IO.Path.Combine(Path, "data.txt");
        // UNDONE: �������� �����
        public static string CurrentFile = System.IO.Path.Combine(Path, "f.txt");
        #endregion
    }
}