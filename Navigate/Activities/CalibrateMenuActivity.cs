using System;
using System.Linq;
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
            //frontButton,
            backButton,
            topButton,
            //bottomButton,
            //leftButton
            rightButton;

        private bool
            //frontCalibrated = false,
            backCalibrated = false,
            topCalibrated = false,
            //bottomCalibrated = false,
            //leftCalibrated = false
            rightCalibrated = false;
        #endregion

        #region Data collecting and processing related fields 
        private const int SamplesToCollect = 300;
        private float[][] samples = new float[SamplesToCollect][];
        private float[] medianSamples;
        private float[][] gyroSamples = new float[SamplesToCollect][];
        private int numberOfSamplesCollected = 0;
        PhoneOrientation currentOrientation;

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

            Storage.StopSensorsDataService();
            sensorManager = (SensorManager)GetSystemService(Context.SensorService);

            calibrateButton = FindViewById<Button>(Resource.Id.CalibrateButton);
            backButton = FindViewById<Button>(Resource.Id.CalibrateBackButton);
            topButton = FindViewById<Button>(Resource.Id.CalibrateTopButton);
            rightButton = FindViewById<Button>(Resource.Id.CalibrateRightButton);

            calibrateButton.Click += OnCalibrateButtonClicked;
            calibrateButton.Enabled = false;

            backButton.Click += OnAnySideButtonClicked;
            topButton.Click += OnAnySideButtonClicked;
            rightButton.Click += OnAnySideButtonClicked;

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = new float[3];
                gyroSamples[i] = new float[3];
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
            Storage.StopSensorsDataService();
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
            else if (e.Sensor.Type == SensorType.Gyroscope)
            {
                if (currentOrientation == PhoneOrientation.OnBack)
                {
                    e.Values.CopyTo(gyroSamples[numberOfSamplesCollected], 0);
                }
            }
        }

        private void EnableSensors()
        {
            numberOfSamplesCollected = 0;
            sensorManager.RegisterListener(this,
                    sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                    SensorDelay.Fastest);
            sensorManager.RegisterListener(this,
                    sensorManager.GetDefaultSensor(SensorType.Gyroscope),
                    SensorDelay.Fastest);
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
            var samplesTransposed = new float[3][];
            
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

            if (currentOrientation == PhoneOrientation.OnBack)
            {
                var gyroMedians = new float[3];
                var gyroSamplesTransposed = new float[3][];
                for (int i = 0; i < 3; i++)
                {
                    gyroSamplesTransposed[i] = new float[SamplesToCollect];
                    for (int j = 0; j < SamplesToCollect; j++)
                    {
                        gyroSamplesTransposed[i][j] = gyroSamples[j][i];
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    Array.Sort(gyroSamplesTransposed[i]);
                    gyroMedians[i] = SamplesToCollect % 2 == 1 ? gyroSamplesTransposed[i][SamplesToCollect] :
                        (gyroSamplesTransposed[i][SamplesToCollect / 2] + gyroSamplesTransposed[i][1 + SamplesToCollect / 2]) * 0.5f;
                }
                Storage.Current.GyroscopeCalibrationVector = new OpenTK.Vector3(gyroMedians[0], gyroMedians[1], gyroMedians[2]);
            }

            MediansCalculated();
        }
        #endregion

        #region Handlers
        private void OnAnySideButtonClicked(object sender, EventArgs e)
        {
            AllSamplesCollected?.GetInvocationList().ToList().ForEach(d => AllSamplesCollected -= d as Action);
            MediansCalculated?.GetInvocationList().ToList().ForEach(d => MediansCalculated -= d as Action);

            var button = sender as Button;
            
            switch (button.Id)
            {
                case Resource.Id.CalibrateBackButton:
                {
                    currentOrientation = PhoneOrientation.OnBack;
                    break;
                }
                case Resource.Id.CalibrateTopButton:
                {
                    currentOrientation = PhoneOrientation.OnTop;
                    break;
                }
                case Resource.Id.CalibrateRightButton:
                {
                    currentOrientation = PhoneOrientation.OnRight;
                    break;
                }
                //case Resource.Id.CalibrateFrontButton:
                //{
                //    orientation = PhoneOrientation.OnFront;
                //    break;
                //}
                //case Resource.Id.CalibrateBottomButton:
                //{
                //    orientation = PhoneOrientation.OnBottom;
                //    break;
                //}
                //case Resource.Id.CalibrateLeftButton:
                //{
                //    orientation = PhoneOrientation.OnLeft;
                //    break;
                //}
                default:
                {
                    currentOrientation = PhoneOrientation.Unknown;
                    break;
                }
            }
            int column = -1;
            if (currentOrientation == PhoneOrientation.OnRight)
            {
                rightCalibrated = true;
                column = 0;
            }
            else if (currentOrientation == PhoneOrientation.OnTop)
            {
                topCalibrated = true;
                column = 1;
            }
            else if (currentOrientation == PhoneOrientation.OnBack)
            {
                backCalibrated = true;
                column = 2;
            }

            // TODO: подождать, пока телефон не будет двигатьс€ с эпсилон погрешностью

            EnableSensors();
            ShowProgressDialog("");
            AllSamplesCollected += GetMedianValues;
            MediansCalculated += () => button.SetBackgroundResource(Resource.Drawable.GreenButton);
            MediansCalculated += () =>
            {
                for (int i = 0; i < 3; i++)
                    calibrateMatrix[column, i] = medianSamples[i] / g;

                if (backCalibrated && topCalibrated && rightCalibrated)
                {
                    calibrateButton.Enabled = true;
                    calibrateButton.SetBackgroundResource(Resource.Drawable.WhiteButton);
                }
            };
        }
        private void OnCalibrateButtonClicked(object sender, EventArgs e)
        {
            calibrateMatrix = calibrateMatrix.Inversed();
            //float[] calebrateValues = new float[9];
            //for (int i = 0; i < 3; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        calebrateValues[i * 3 + j] = calibrateMatrix[i, j];
            //    }
            //}
            //Storage.Current.AccelerometerCalibrationMatrix = new OpenTK.Matrix3(calebrateValues);

            Storage.Current.AccelerometerCalibrationMatrix = calibrateMatrix;

            calibrateButton.Text = calibrateMatrix.ToString();
        }
        #endregion
    }
}