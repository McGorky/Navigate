using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Hardware;
using Android.Widget;
using Android.Runtime;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "CalibrateMenu",
        Theme = "@style/DarkRedAndPink")]
    public class CalibrateMenuActivity : Activity, ISensorEventListener
    {
        // CONSIDER: использовать данные GPS дл€ определени€ g
        // по формуле g = 9.780318*(1+0.005302*sin(p) - 0.000006*sin(2*p)^2) - 0.000003086*h
        // p - широта, h - высота над уровнем мор€ в метрах
        private const float g = 9.8154f; // «начение дл€ ћосквы

        private SensorManager sensorManager;

        private ProgressDialog dataCollectingDialog;

        #region Views and related fields
        private Button calibrateButton,
            frontButton,
            backButton,
            topButton,
            bottomButton,
            leftButton,
            rightButton;

        private bool
            frontCalibrated = false,
            backCalibrated = false,
            topCalibrated = false,
            bottomCalibrated = false,
            leftCalibrated = false,
            rightCalibrated = false;
        #endregion

        #region Data collecting and processing related fields 
        private const int SamplesToCollect = 300;
        private float[][] samples = new float[SamplesToCollect][];
        private float[] medianSamples;
        private int numberOfSamplesCollected = 0;

        private Matrix expectedValues = new Matrix(4, 4);
        private Matrix actualValues = new Matrix(4, 4);

        private Matrix calibrateMatrix = new Matrix(3, 3);

        // CONSIDER: использовать EventArgs
        private event Action AllSamplesCollected;
        private event Action MediansCalculated;
        private event Action<int> SomeSamplesCollected;
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.CalibrateMenu);

            sensorManager = (SensorManager)GetSystemService(Context.SensorService);

            calibrateButton = FindViewById<Button>(Resource.Id.CalibrateButton);
            frontButton = FindViewById<Button>(Resource.Id.CalibrateFrontButton);
            backButton = FindViewById<Button>(Resource.Id.CalibrateBackButton);
            topButton = FindViewById<Button>(Resource.Id.CalibrateTopButton);
            bottomButton = FindViewById<Button>(Resource.Id.CalibrateBottomButton);
            leftButton = FindViewById<Button>(Resource.Id.CalibrateLeftButton);
            rightButton = FindViewById<Button>(Resource.Id.CalibrateRightButton);

            calibrateButton.Click += OnCalibrateButtonClicked;

            frontButton.Click += OnAnySideButtonClicked;
            backButton.Click += OnAnySideButtonClicked;
            topButton.Click += OnAnySideButtonClicked;
            bottomButton.Click += OnAnySideButtonClicked;
            leftButton.Click += OnAnySideButtonClicked;
            rightButton.Click += OnAnySideButtonClicked;

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = new float[3];
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }
        #endregion

        #region Data collecting and processing related methods
        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Type == SensorType.Accelerometer)
            {
                e.Values.CopyTo(samples[numberOfSamplesCollected], 0);
                numberOfSamplesCollected++;
                SomeSamplesCollected(numberOfSamplesCollected);
                if (numberOfSamplesCollected == SamplesToCollect)
                {
                    DisableSensors();
                    AllSamplesCollected();
                }
            }
        }

        private void EnableSensors()
        {
            numberOfSamplesCollected = 0;
            sensorManager.RegisterListener(this,
                    sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                    SensorDelay.Game);
        }

        private void DisableSensors()
        {
            sensorManager.UnregisterListener(this);
        }

        private void ShowProgressDialog(string message)
        {
            dataCollectingDialog = new ProgressDialog(this);
            dataCollectingDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            dataCollectingDialog.SetMessage(message + GetString(Resource.String.CollectingData));
            dataCollectingDialog.SetCancelable(false);
            dataCollectingDialog.Progress = 0;
            dataCollectingDialog.Max = SamplesToCollect;
            dataCollectingDialog.Show();

            SomeSamplesCollected += (i) => dataCollectingDialog.Progress = i;
            AllSamplesCollected += () => RunOnUiThread(() => dataCollectingDialog.Dismiss());
        }

        private void GetMedianValues()
        {
            medianSamples = new float[3];
            float[][] samplesTransposed = new float[3][];
            for (int i = 0; i < 3; i++)
            {
                samplesTransposed[i] = new float[SamplesToCollect];
                for (int j = 0; j < SamplesToCollect; j++)
                {
                    samplesTransposed[i][j] = samples[j][i];
                }
            }
            for (int i = 0; i < 3; i++)
            {
                Array.Sort(samplesTransposed[i]);
                medianSamples[i] = SamplesToCollect % 2 == 1 ? samplesTransposed[i][SamplesToCollect] :
                    (samplesTransposed[i][SamplesToCollect / 2] + samplesTransposed[i][1 + SamplesToCollect / 2]) * 0.5f;
            }

            MediansCalculated();
        }
        #endregion

        #region Handlers
        private void OnAnySideButtonClicked(object sender, EventArgs e)
        {
            var handlers = AllSamplesCollected?.GetInvocationList();
            if (handlers != null)
            {
                foreach (var h in handlers)
                {
                    AllSamplesCollected -= h as Action;
                }
            }
            handlers = MediansCalculated?.GetInvocationList();
            if (handlers != null)
            {
                foreach (var h in handlers)
                {
                    MediansCalculated -= h as Action;
                }
            }

            var button = sender as Button;
            PhoneOrientation orientation;
            switch (button.Id)
            {
                case Resource.Id.CalibrateFrontButton:
                {
                    orientation = PhoneOrientation.OnFront;
                    break;
                }
                case Resource.Id.CalibrateBackButton:
                {
                    orientation = PhoneOrientation.OnBack;
                    break;
                }
                case Resource.Id.CalibrateTopButton:
                {
                    orientation = PhoneOrientation.OnTop;
                    break;
                }
                case Resource.Id.CalibrateBottomButton:
                {
                    orientation = PhoneOrientation.OnBottom;
                    break;
                }
                case Resource.Id.CalibrateLeftButton:
                {
                    orientation = PhoneOrientation.OnLeft;
                    break;
                }
                case Resource.Id.CalibrateRightButton:
                {
                    orientation = PhoneOrientation.OnRight;
                    break;
                }
                default:
                {
                    orientation = PhoneOrientation.Unknown;
                    break;
                }
            }
            int column = -1;
            if (orientation == PhoneOrientation.OnRight)
                column = 0;
            else if (orientation == PhoneOrientation.OnTop)
                column = 1;
            else if (orientation == PhoneOrientation.OnBack)
                column = 2;

            // TODO: подождать, пока телефон не будет двигатьс€ с эпсилон погрешностью

            EnableSensors();
            ShowProgressDialog(orientation.ToString());
            AllSamplesCollected += GetMedianValues;
            MediansCalculated += () => button.SetBackgroundResource(Resource.Drawable.GreenButton);
            MediansCalculated += () =>
            {
                for (int i = 0; i < 3; i++)
                    calibrateMatrix[column, i] = medianSamples[i] / g;
            };
        }
        private void OnCalibrateButtonClicked(object sender, EventArgs e)
        {
            calibrateButton.SetBackgroundResource(Resource.Drawable.WhiteButton);
            //Storage.AccelerometerCalibrationMatrix = expectedValues * actualValues.Inversed();
            calibrateMatrix = calibrateMatrix.Inversed();

            calibrateButton.Text = calibrateMatrix.ToString();
        }
        #endregion
    }
}