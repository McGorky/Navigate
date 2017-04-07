using System;
using System.Text;
using System.Timers;
using System.IO;
using System.Globalization;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Hardware;
using Android.Widget;
using Android.Runtime;

namespace Mirea.Snar2017.Navigate
{
    [Service]
    public class SensorsDataService : Service, ISensorEventListener
    {
        // REFACTOR: привести в порядок поля
        private SensorManager sensorManager;
        private Timer timer = new Timer();
        private Timer starter = new Timer();
        StreamWriter streamWriter;
        StringBuilder builder = new StringBuilder();
        bool record = false;
        DateTime st;
        public override void OnCreate()
        {
            base.OnCreate();
            StartForeground(Storage.ForegroundServiceId.SenorsData, new Notification());

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            //File.SetAttributes(Storage.Filename, FileAttributes.Normal);
            streamWriter = new StreamWriter(Storage.Filename, false);
            starter.Elapsed += (o, e) =>
            {
                record = true;
                timer.Interval = 20;
                streamWriter.WriteLine(Storage.AccelerometerCalibrationMatrix.ToString());
                st = DateTime.Now;
                streamWriter.WriteLine(st.ToString());
                timer.Enabled = true;
            };
            starter.Interval = 500;
            starter.Enabled = true;
            starter.AutoReset = false;

            timer.Elapsed += (o, e) =>
            {
                lock (Storage.DataAccessSync)
                {
                    builder.Append($"{DateTime.Now.Subtract(st).TotalMilliseconds.ToString()},");
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
            timer.Enabled = false;

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
            streamWriter.Dispose();
            StopForeground(true);
            base.OnDestroy();
        }


        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (record)
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