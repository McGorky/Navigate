using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Media;

namespace Mirea.Snar2017.Navigate
{
    [Service]
    public class SensorsDataService : Service, ISensorEventListener
    {
        private SensorManager sensorManager;
        private Timer dataWriteTimer = new Timer();
        private Timer startTimer = new Timer();
        StreamWriter streamWriter;
        FileStream fileStream;
        StringBuilder builder = new StringBuilder();
        bool isRecording = false;
        DateTime startTime;

        public override void OnCreate()
        {
            base.OnCreate();
            StartForeground(Storage.ForegroundServiceId.SenorsData, new Notification());

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(Storage.RawFolderName);
            fileStream = new FileStream(Storage.CurrentRawFile, FileMode.OpenOrCreate, FileAccess.Write);
            streamWriter = new StreamWriter(fileStream); 
            startTimer.Elapsed += (o, e) =>
            {
                streamWriter.WriteLine(Storage.AccelerometerCalibrationMatrix.ToString());
                Storage.StartTime = DateTime.Now;
                streamWriter.WriteLine(startTime.ToString());
                isRecording = true;
                dataWriteTimer.Interval = 20;
                dataWriteTimer.Enabled = true;
            };
            startTimer.Interval = 500;
            startTimer.Enabled = true;
            startTimer.AutoReset = false;

            dataWriteTimer.Elapsed += (o, e) =>
            {
                lock (Storage.DataAccessSync)
                {
                    builder.Append($"{DateTime.Now.Subtract(Storage.StartTime).TotalMilliseconds.ToString()},");
                    for (int i = 0; i < 3; i++)
                        builder.Append($"{Storage.AccelerometerData[i]},");
                    for (int i = 0; i < 3; i++)
                        builder.Append($"{Storage.GyroscopeData[i]},");
                    for (int i = 0; i < 3; i++)
                        builder.Append($"{Storage.MagnetometerData[i]},");
                    for (int i = 0; i < 3; i++)
                        builder.Append($"{Storage.LinearAccelerationData[i]}{(i < 2 ? "," : "")}");
                    streamWriter.WriteLine(builder.ToString());
                    builder.Clear();
                }
            };
            dataWriteTimer.Enabled = false;

            Storage.StartTime = DateTime.Now;
            sensorManager = (SensorManager)GetSystemService(Context.SensorService);
            sensorManager.RegisterListener(this,
                                sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                                SensorDelay.Game);

            // TODO: использовать некалиброванные данные
            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.Gyroscope),
                                            SensorDelay.Game);

            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.MagneticField),
                                            SensorDelay.Game);
            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.LinearAcceleration),
                                            SensorDelay.Game);
        }

        // TODO: разобраться с этим
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.NotSticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            sensorManager.UnregisterListener(this);
            streamWriter.Close();
            streamWriter.Dispose();
            fileStream.Close();
            fileStream.Dispose();
            MediaScannerConnection.ScanFile(this, new string[] { Storage.RawFolderName }, null, null);
            StopForeground(true);
            base.OnDestroy();
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (isRecording)
            {
                Storage.Uptime = DateTime.Now.Subtract(Storage.StartTime);
                switch (e.Sensor.Type)
                {
                    case SensorType.Accelerometer:
                    {
                        for (int i = 0; i < 3; i++)
                            Storage.AccelerometerData[i] = e.Values[i];
                        break;
                    }
                    case SensorType.Gyroscope:
                    {
                        for (int i = 0; i < 3; i++)
                            Storage.GyroscopeData[i] = e.Values[i];
                        break;
                    }
                    case SensorType.MagneticField:
                    {
                        for (int i = 0; i < 3; i++)
                            Storage.MagnetometerData[i] = e.Values[i];
                        break;
                    }
                    case SensorType.LinearAcceleration:
                    {
                        for (int i = 0; i < 3; i++)
                            Storage.LinearAccelerationData[i] = e.Values[i];
                        break;
                    }
                }
            }
        }
    }
}