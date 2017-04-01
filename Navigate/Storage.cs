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
    public class Storage
    {
        public static object SyncLock = new object();

        /*public static Quaternion AccelerometerData = (0, 0.01f, 0.01f, 0.01f);
        public static Quaternion GyroscopeData = (0, 0.01f, 0.01f, 0.01f);
        public static Quaternion MagnetometerData = (0, 0.01f, 0.01f, 0.01f);*/

        public static class ForegroundServiceId
        {
            public static int SenorsData = 812563123;
        }

        public static int currentFrame = 0;
        public static int numberOfFrames;
        public static float[][] data;

        public static float[] AccelerometerData = new float[3];
        public static float[] GyroscopeData = new float[3];
        public static float[] MagnetometerData = new float[3];
        public static float[] LinearAccelerationData = new float[3];

        public static string Path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);//(string)Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);//(string)Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments); 
        public static string Filename = System.IO.Path.Combine(Path, "data.txt");

        public static Matrix AccelerometerCalibrationMatrix;

        public static Quaternion StartRotation = (1, 0, 0, 0);
        public static Quaternion Rotation = (1, 0, 0, 0);

        public static string CurrentFile = System.IO.Path.Combine(Path, "f.txt");

        public static (float psi, float theta, float phi) EulerAngles;

        public static float[] Acceleration = new float[3];
        public static float[] Velocity = new float[3];
        public static float[] Offset = new float[3];

        public static float Beta = 0.1f;
        public static float Zeta;
        public static float Gamma = 0.7f;

        public static System.TimeSpan Uptime = new System.TimeSpan();

        public static System.DateTime StartTime;
    }
}