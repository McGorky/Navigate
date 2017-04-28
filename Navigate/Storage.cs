using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.App;
using Android.Hardware;
using Android.Media;

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using Mirea.Snar2017.Navigate.Services;

namespace Mirea.Snar2017.Navigate
{
    // REMARK ALL: код должен выглядеть идеально и должен читаться с первого раза
    // REMARK ALL: используйте #region - #endregion для того, чтобы сгруппировать код (не использовать внутри методов)
    // REMARK ALL: давайте всем переменным адекватные имена сразу, чтобы потом не приходилось заниматься рефакторингом https://msdn.microsoft.com/ru-ru/library/ms229002(v=vs.110).aspx
    // REMARK ALL: не задумывайтесь над оптимизацией

    [DataContract]
    public class Storage
    {
        //public static object DataAccessSync { get; set; } = new object();

        #region SensorsDataService related
        public static Storage Current { get; }

        public SensorsDataService SensorsDataService
        {
            get
            {
                if (sensorsDataServiceConnection.Binder == null)
                    throw new Exception("Service not bound yet");

                return sensorsDataServiceConnection.Binder.Service;
            }
        }

        public event EventHandler<ServiceConnectedEventArgs> SensorsDataServiceConnected = delegate { };

        private static SensorsDataServiceConnection sensorsDataServiceConnection;

        static Storage()
        {
            Current = new Storage();
            try
            {
                using (var fileStream = new FileStream(SettingsFileName, FileMode.Open, FileAccess.Read))
                {
                    var jsonFormatter = new DataContractJsonSerializer(typeof(Storage));
                    Current = (Storage)jsonFormatter.ReadObject(fileStream);
                }
            }
            catch (FileNotFoundException)
            {
            }
            sensorsDataServiceConnection = new SensorsDataServiceConnection(null);
            sensorsDataServiceConnection.ServiceConnected += (object s, ServiceConnectedEventArgs e) => Current.SensorsDataServiceConnected(null, e);
        }

        private Storage()
        {
        }

        public static void StartSensorsDataService()
        {
            Task.Run(() =>
            {
                Android.App.Application.Context.StartService(new Intent(Android.App.Application.Context, typeof(SensorsDataService)));
                var locationServiceIntent = new Intent(Android.App.Application.Context, typeof(SensorsDataService));
                Android.App.Application.Context.BindService(locationServiceIntent, sensorsDataServiceConnection, Bind.AutoCreate);
            });
        }

        public static void StopSensorsDataService()
        {
            if (sensorsDataServiceConnection != null)
            {
                Android.App.Application.Context.UnbindService(sensorsDataServiceConnection);
            }

            if (Current.SensorsDataService != null)
            {
                Current.SensorsDataService.StopSelf();
            }
        }

        public void SaveSettings(Context context)
        {
            using (var fileStream = new FileStream(SettingsFileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var jsonFormatter = new DataContractJsonSerializer(typeof(Storage));
                jsonFormatter.WriteObject(fileStream, this);
                MediaScannerConnection.ScanFile(context, new string[] { Storage.ApllicationDataFolder }, null, null);
            }
        }
        #endregion

        #region LogPlayer data
        // UNDONE: разобраться
        public static int CurrentFrame = 0;
        public static int NumberOfFrames = 0;
        public static float[][] Data = null;

        public static bool TrajectoryTracingEnabled { get; set; } = true;
        #endregion

        #region Raw data
        //public static float[] AccelerometerData { get; } = new float[3];
        //public static float[] LinearAccelerationData { get; } = new float[3];
        //public static float[] GyroscopeData { get; } = new float[3];
        //public static float[] GyroscopeUncalibratedData { get; } = new float[3];
        //public static float[] MagnetometerData { get; } = new float[3];

        public static long StartTimestamp;
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
        #endregion

        #region Filter parameters
        [DataMember]
        public OpenTK.Matrix3 AccelerometerCalibrationMatrix { get; set; } = OpenTK.Matrix3.Identity;
        [DataMember]
        public OpenTK.Vector3 GyroscopeCalibrationVector { get; set; } = new OpenTK.Vector3();

        public Quaternion StartRotation { get; set; } = (1, 0, 0, 0);

        [DataMember]
        public float Beta { get; set; } = 0.1f;
        [DataMember]
        public float Zeta { get; set; } = 0.1f;
        [DataMember]
        public float Gamma { get; set; } = 0.7f;

        [DataMember]
        public bool MagnetometerEnabled { get; set; } = true;
        [DataMember]
        public bool GyroscopeDriftCompensationEnabled { get; set; } = false;
        [DataMember]
        public bool AccelerometerCalibrationEnabled { get; set; } = true;
        [DataMember]
        public bool GyroscopeCalibrationEnabled { get; set; } = false;

        public static TimeSpan Uptime { get; set; } = new TimeSpan();
        public static DateTime StartTime { get; set; }
        #endregion

        #region File parameters
        //public static string ApllicationDataFolder { get; set; } = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        public static string ApllicationDataFolder { get; set; } = Path.Combine((string)Android.OS.Environment.ExternalStorageDirectory, "Navigate");
        public static string CurrentFilename { get; set; }// = "data.txt";
        public static string RawFolderName { get; } = Path.Combine(ApllicationDataFolder, "raw");
        public static string FilteredFolderName { get; } = Path.Combine(ApllicationDataFolder, "filtered");
        public static string SettingsFileName { get; } = Path.Combine(ApllicationDataFolder, "settings.json");
        public static string CurrentRawFile { get => Path.Combine(RawFolderName, CurrentFilename); }
        public static string CurrentFilteredFile { get => Path.Combine(FilteredFolderName, CurrentFilename); }
        #endregion

        public static class ForegroundServiceId
        {
            public static int SensorsData = 456462365;
        }
    }
}