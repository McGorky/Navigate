using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

//using OpenTK;

using static Mirea.Snar2017.Navigate.Storage;

namespace Mirea.Snar2017.Navigate
{
    [Service]
    class FilterService : Service
    {
        private int UpDateTime = 50; // ms
        private Timer timer = new Timer();
        private Quaternion orientation;

        public override void OnCreate()
        {
            base.OnCreate();
            // TODO: сделать нормальный id
            StartForeground(123346243, new Notification());
            timer.Elapsed += (o, e) =>
            {
                var time = DateTime.Now;
                var dt = (float)time.Subtract(StartTime).TotalSeconds;
                var a = Filter.Calibrate(AccelerometerCalibrationMatrix, AccelerometerData);
                var g = new Quaternion(0, GyroscopeData[0], GyroscopeData[1], GyroscopeData[2]);
                var m = new Quaternion(0, MagnetometerData[0], MagnetometerData[1], MagnetometerData[2]);
                Filter.Madgvic(ref orientation, g, a, m, Beta, dt);
                // TODO: вычисление перемещения
                // TODO: занесение времени в файл
            };
            timer.Interval = UpDateTime;
            timer.Enabled = true;
        }

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
            StopForeground(true);
            base.OnDestroy();
        }
    }
}