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
        // TODO: привести в порядок поля
        SensorManager sensorManager;
        //TextView calibrateTextView;
        Stopwatch stopwatch = new Stopwatch();

        Button calibrateButton, frontButton, backButton, topButton, bottomButton, leftButton, rightButton;

        private bool frontbool, topbool, backbool, bottombool, leftbool, rightbool = false;

        private bool isGathering = false;
        private static int samplesToGather = 300;
        private float[,] samples = new float[samplesToGather, 3];
        private float[] meanSample;
        private int samplesGathered = 0;
        private PhoneOrientation orientaion;
        private Matrix R = new Matrix(4, 4);
        private Matrix r = new Matrix(4, 4);

        private event Action SamplesGathered;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            sensorManager = (SensorManager)GetSystemService(Context.SensorService);

            SetContentView(Resource.Layout.CalibrateMenu);

           // calibrateTextView = FindViewById<TextView>(Resource.Id.CalibrateTextView);

            calibrateButton = FindViewById<Button>(Resource.Id.CalibrateButton);
            frontButton = FindViewById<Button>(Resource.Id.CalibrateFrontButton);
            backButton = FindViewById<Button>(Resource.Id.CalibrateBackButton);
            topButton = FindViewById<Button>(Resource.Id.CalibrateTopButton);
            bottomButton = FindViewById<Button>(Resource.Id.CalibrateBottomButton);
            leftButton = FindViewById<Button>(Resource.Id.CalibrateLeftButton);
            rightButton = FindViewById<Button>(Resource.Id.CalibrateRightButton);

            calibrateButton.Click += calibrateButtonClicked;
            frontButton.Click += frontButtonClicked;
            backButton.Click += backButtonClicked;
            topButton.Click += topButtonClicked;
            bottomButton.Click += bottomButtonClicked;
            leftButton.Click += leftButtonClicked;
            rightButton.Click += rightButtonClicked;

            float g = 9.81f;
            R[0, 2] = g;
            R[1, 3] = -g;
            R[2, 0] = -g;
            R[2, 1] = g;
            R[3, 0] = 1;
            R[3, 1] = 1;
            R[3, 2] = 1;
            R[3, 3] = 1;

            StopService(new Intent(this, typeof(SensorsDataService)));
            sensorManager.RegisterListener(this,
                    sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                    SensorDelay.Game);
        }

        protected override void OnDestroy()
        {
            sensorManager.UnregisterListener(this);
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

        private void CollectData()
        {
            samplesGathered = 0;
            stopwatch.Start();
            isGathering = true;
            SamplesGathered += ManageData;
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
            stopwatch.Stop();
          //  RunOnUiThread(() => calibrateTextView.Text = $"{stopwatch.Elapsed.TotalSeconds} \n {meanSample[0]} | {meanSample[1]} | {meanSample[2]}");
            stopwatch.Reset();
            int column = -1;
            switch (orientaion)
            {
                case PhoneOrientation.OnFront:
                    column = 0;
                    break;
                case PhoneOrientation.OnBack:
                    column = 1;
                    break;
                case PhoneOrientation.OnLeft:
                    column = 2;
                    break;
                case PhoneOrientation.OnTop:
                    column = 3;
                    break;
            }
            for (int i = 0; i < 3; i++)
                r[i, column] = meanSample[i];

            r[3, column] = 1;
           // calibrateTextView.Text = r.ToString();
        }

        private void ShowProgressBar()
        {
            ProgressDialog progressBar = new ProgressDialog(this);
            progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progressBar.SetMessage("Collecting data...");
            progressBar.SetCancelable(false);
            progressBar.Progress = 0;
            progressBar.Max = samplesToGather;
            progressBar.Show();

            new Thread(new ThreadStart(() =>
            {
                while (progressBar.Progress < progressBar.Max)
                {
                    progressBar.Progress = samplesGathered;
                    Thread.Sleep(100);
                }
                RunOnUiThread(() => progressBar.Dismiss());
            })).Start();
        }

        void frontButtonClicked(object sender, EventArgs e)
        {
            frontbool = true;
            ShowProgressBar();
            orientaion = PhoneOrientation.OnFront;
            CollectData();
            frontButton.SetBackgroundResource(Resource.Drawable.GreenButtonDef);
        }

        void backButtonClicked(object sender, EventArgs e)
        {
            backbool = true;
            ShowProgressBar();
            orientaion = PhoneOrientation.OnBack;
            CollectData();
            backButton.SetBackgroundResource(Resource.Drawable.GreenButtonDef);
        }

        void topButtonClicked(object sender, EventArgs e)
        {
            topbool = true;
            ShowProgressBar();
            orientaion = PhoneOrientation.OnTop;
            CollectData();
            topButton.SetBackgroundResource(Resource.Drawable.GreenButtonDef);
        }

        void bottomButtonClicked(object sender, EventArgs e)
        {
            bottombool = true;
            ShowProgressBar();
            bottomButton.SetBackgroundResource(Resource.Drawable.GreenButtonDef);
        }

        void leftButtonClicked(object sender, EventArgs e)
        {
            leftbool = true;
            ShowProgressBar();
            orientaion = PhoneOrientation.OnLeft;
            CollectData();
            leftButton.SetBackgroundResource(Resource.Drawable.GreenButtonDef);
        }

        void rightButtonClicked(object sender, EventArgs e)
        {
            rightbool = true;
            ShowProgressBar();
            rightButton.SetBackgroundResource(Resource.Drawable.GreenButtonDef);
        }

        void calibrateButtonClicked(object sender, EventArgs e)
        {
            if (frontbool && topbool && backbool && bottombool && leftbool && rightbool)
            {
                calibrateButton.SetBackgroundResource(Resource.Drawable.WhiteButton);
                Storage.AccelerometerCalibrationMatrix = R * r.Inversed();
                // calibrateTextView.Text = Storage.AccelerometerCalibrationMatrix.ToString();
            }

        }
    }
}