using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Hardware;
using Android.Widget;
using Android.Runtime;

using System.Diagnostics;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "CalibrateMenu",
        Theme = "@style/DarkAndGray")]
    public class CalibrateMenuActivity : Activity, ISensorEventListener
    {
        SensorManager sensorManager;
        TextView calibrateTextView;
        Stopwatch s = new Stopwatch();

        private bool isGathering = false;
        private static int samplesToGather = 300;
        private float[,] samples = new float[samplesToGather, 3];
        private float[] meanSample;
        private int samplesGathered = 0;
        private PhoneOrientation orientaion;
        private Matrix R;

        private event Action SamplesGathered;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            sensorManager = (SensorManager)GetSystemService(Context.SensorService);

            SetContentView(Resource.Layout.CalibrateMenu);
            calibrateTextView = FindViewById<TextView>(Resource.Id.CalibrateTextView);
            
            Button calibrateXButton = FindViewById<Button>(Resource.Id.CalibrateXButton);
            Button calibrateYButton = FindViewById<Button>(Resource.Id.CalibrateYButton);
            Button calibrateZButton = FindViewById<Button>(Resource.Id.CalibrateZButton);

            float g = 9.81f;
            R[0, 2] = g;
            R[1, 3] = g;
            R[2, 0] = g;
            R[2, 1] = -g;
            R[3, 0] = 1;
            R[3, 1] = 1;
            R[3, 2] = 1;
            R[3, 3] = 1;

            StopService(new Intent(this, typeof(SensorsDataService)));
            sensorManager.RegisterListener(this,
                    sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                    SensorDelay.Game);

            calibrateXButton.Touch += (o, e) =>
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    // TODO: добавить текущую ориентацию телефона
                    s.Start();
                    isGathering = true;
                    SamplesGathered += ManageData;
                }
            };
        }

        protected override void OnDestroy()
        {
            sensorManager.UnregisterListener(this);
            StartService(new Intent(this, typeof(SensorsDataService)));
            base.OnDestroy();
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (isGathering)
            {
                if  (e.Sensor.Type == SensorType.Accelerometer)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        samples[samplesGathered, i] = e.Values[i];
                    }
                    samplesGathered++;
                    if (samplesGathered == samplesToGather)
                        SamplesGathered();
                }
            }
        }

        private void ManageData()
        {
            // TODO: обработка в зависимости от ориентации
            var meanSample = new float[3];
            for (int i = 0; i < samplesToGather; i++)
                for (int j = 0; j < 3; j++)
                    meanSample[j] += samples[i, j] / samplesToGather;

            isGathering = false;
            SamplesGathered -= ManageData; 
            samplesGathered = 0;
            s.Stop();
            RunOnUiThread(() => calibrateTextView.Text = $"{s.Elapsed.TotalSeconds} \n {meanSample[0]} | {meanSample[1]} | {meanSample[2]}");
            s.Reset();
        }
    }
}