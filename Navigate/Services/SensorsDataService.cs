using System;
using System.Threading;
using System.Threading.Tasks;

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
        private SensorManager sensorManager;
        
        private object syncLock = new object();

        public override void OnCreate()
        {
            base.OnCreate();
            // TODO: сделать нормальный id
            StartForeground(21415213, new Notification());
            Storage.StartTime = DateTime.Now;
            sensorManager = (SensorManager)GetSystemService(Context.SensorService);
            sensorManager.RegisterListener(this,
                                sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                                SensorDelay.Game);

            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.Gyroscope),
                                            SensorDelay.Game);

            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.MagneticField),
                                            SensorDelay.Game);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.NotSticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            // This is a started service, not a bound service, so we just return null.
            return null;
        }

        public override void OnDestroy()
        {
            sensorManager.UnregisterListener(this);
            StopForeground(true);
            base.OnDestroy();
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            // TODO: нужен ли тут lock
            //lock (syncLock)
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
                        break;
                    }
                }
            }
        }

    }
}