using System;
using System.IO;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Mirea.Snar2017.Navigate
{
    // REMARK ALL: код должен выглядеть идеально и должен читаться с первого раза
    // REMARK ALL: используйте #region - #endregion для того, чтобы сгруппировать код (не использовать внутри методов)
    // REMARK ALL: давайте всем переменным адекватные имена сразу, чтобы потом не приходилось заниматься рефакторингом https://msdn.microsoft.com/ru-ru/library/ms229002(v=vs.110).aspx
    // REMARK ALL: не задумывайтесь над оптимизацией

    // CONSIDER: переименовать
    public class Storage
    {
        public static class ForegroundServiceId
        {
            public static int SenorsData = 812563123;
        }

        public static object DataAccessSync { get; set; } = new object();

        #region LogPlayer data
        // UNDONE: разобраться
        public static int CurrentFrame = 0;
        public static int NumberOfFrames = 0;
        public static float[][] Data = null;

        public static bool TrajectoryTracingEnabled { get; set; } = true;
        #endregion

        #region Raw data
        // CONSIDER: разобраться, нужен ли тут set
        public static float[] AccelerometerData { get; } = new float[3];
        public static float[] GyroscopeData { get; } = new float[3];
        public static float[] MagnetometerData { get; } = new float[3];
        // TODO: использовать только данные акселерометра для определения перемещений
        public static float[] LinearAccelerationData { get; } = new float[3];

        //public static (float X, float Y, float Z) AccelerometerData { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) GyroscopeData { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) MagnetometerData { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) LinearAccelerationData { get; } = (0, 0, 0);
        #endregion

        #region Plot data
        /*public static LineSeries AccelerometerX = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Red, MarkerSize = 0.5};
        public static LineSeries AccelerometerY = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Red, MarkerSize = 0.5 };
        public static LineSeries AccelerometerZ = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Red, MarkerSize = 0.5 };
        public static LineSeries Gyro;
        public static LineSeries Magnetometer;
        public static float[] SensorsTimes;*/

        public static LineSeries Phi = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Blue, MarkerSize = 0.3 };
        public static LineSeries Theta = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Blue, MarkerSize = 0.3 };
        public static LineSeries Psi = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Blue, MarkerSize = 0.3 };
        /*public static float[][] Offsets;
        public static float[] OffsetsTimes;*/

        public static (float Psi, float Theta, float Phi) ToEulerAngles(Quaternion q)
        {
            var psi = (float)Math.Atan2(2 * q[1] * q[4] + 2 * q[2] * q[3], 1 - 2 * q[3] * q[3] - 2 * q[4] * q[4]);
            var theta = (float)-Math.Asin(2 * q[1] * q[3] - 2 * q[4] * q[2]);
            var phi = (float)Math.Atan2(2 * q[1] * q[2] + 2 * q[3] * q[4], 1 - 2 * q[2] * q[2] - 2 * q[3] * q[3]);

            return (psi, theta, phi);
        }
        #endregion

        #region Filtered data
        public static Quaternion Rotation { get; set; } = (1, 0, 0, 0);

        public static float[] Acceleration { get; } = new float[3];
        public static float[] Velocity { get; } = new float[3];
        public static float[] Offset { get; } = new float[3];

        //public static (float X, float Y, float Z) Acceleration { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) Velocity { get; } = (0, 0, 0);
        //public static (float X, float Y, float Z) Offset { get; } = (0, 0, 0);
        #endregion

        #region Filter parameters
        public static Matrix AccelerometerCalibrationMatrix { get; set; } = new Matrix(4, 4, MatrixInitializationValue.Identity);
        public static float[] GyroscopeCalibrationVector { get; set; } = new float[3];

        public static Quaternion StartRotation { get; set; } = (1, 0, 0, 0);

        public static float Beta { get; set; } = 0.1f;
        public static float Zeta { get; set; } = 0.1f;
        public static float Gamma { get; set; } = 0.7f;

        public static bool MagnetometerEnabled { get; set; } = true;
        public static bool GyroscopeDriftCompensationEnabled { get; set; } = false;
        public static bool AccelerometerCalibrationEnabled { get; set; } = true;
        public static bool GyroscopeCalibrationEnabled { get; set; } = false;

        public static TimeSpan Uptime { get; set; } = new TimeSpan();
        public static DateTime StartTime { get; set; }
        #endregion

        #region File parameters
        //public static string ApllicationDataFolder { get; set; } = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        public static string ApllicationDataFolder { get; set; } = Path.Combine((string)Android.OS.Environment.ExternalStorageDirectory, "Navigate");
        public static string Filename { get; set; }// = "data.txt";
        public static string RawFolderName { get; } = Path.Combine(ApllicationDataFolder, "raw");
        public static string FilteredFolderName { get; } = Path.Combine(ApllicationDataFolder, "filtered");
        public static string CurrentRawFile { get => Path.Combine(RawFolderName, Filename); }
        public static string CurrentFilteredFile { get => Path.Combine(FilteredFolderName, Filename); }
        #endregion
    }
}