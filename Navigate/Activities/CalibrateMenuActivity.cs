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
        private float[,] samples = new float[SamplesToCollect, 3];
        private float[] meanSample;
        private int numberOfSamplesCollected = 0;

        private PhoneOrientation orientaion;

        private Matrix R = new Matrix(4, 4);
        private Matrix r = new Matrix(4, 4);

        // CONSIDER: использовать EventArgs
        private event Func<Task<float[]>> AllSamplesCollected;
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
            frontButton.Click += OnFrontButtonClicked;
            backButton.Click += OnBackButtonClicked;
            topButton.Click += OnTopButtonClicked;
            bottomButton.Click += OnBottomButtonClicked;
            leftButton.Click += OnLeftButtonClicked;
            rightButton.Click += OnRightButtonClicked;

            
            R[0, 2] = g;
            R[1, 3] = -g;
            R[2, 0] = -g;
            R[2, 1] = g;
            R[3, 0] = 1;
            R[3, 1] = 1;
            R[3, 2] = 1;
            R[3, 3] = 1;
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
                for (int i = 0; i < 3; i++)
                {
                    samples[numberOfSamplesCollected, i] = e.Values[i];
                }
                numberOfSamplesCollected++;
                if (numberOfSamplesCollected % 10 == 0)
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
            StopService(new Intent(this, typeof(SensorsDataService)));
            sensorManager.RegisterListener(this,
                    sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                    SensorDelay.Game);
        }

        private void DisableSensors()
        {
            sensorManager.UnregisterListener(this);
        }

        private async Task CollectData(PhoneOrientation orientation)
        {
            ShowProgressDialog();
            SomeSamplesCollected += UpdateProgressDialog;
            numberOfSamplesCollected = 0;
            EnableSensors();
            AllSamplesCollected += this.ProcessData;
            var ProcessData = (Func<Task<float[]>>)AllSamplesCollected.GetInvocationList()[0];
            var data = await ProcessData();
            DismissProgressDialog();
        }
        // TODO: вызывать событие при считывании каждого 10 файла
        private async Task<float[]> ProcessData()
        {
            var meanSample = new float[3];
            await Task.Run(() =>
            {
                for (int i = 0; i < SamplesToCollect; i++)
                    for (int j = 0; j < 3; j++)
                        meanSample[j] += samples[i, j] / SamplesToCollect;

            });

            AllSamplesCollected -= ProcessData;
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

            return meanSample;
        }

        private void ShowProgressDialog()
        {
            dataCollectingDialog = new ProgressDialog(this);
            dataCollectingDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            dataCollectingDialog.SetMessage(GetString(Resource.String.CollectingData));
            dataCollectingDialog.SetCancelable(false);
            dataCollectingDialog.Progress = 0;
            dataCollectingDialog.Max = SamplesToCollect;
            dataCollectingDialog.Show();
            /*Task.Run(async () =>
            {
                
            });*/
            /*new Thread(new ThreadStart(() =>
            {
                while (dataCollectingDialog.Progress < dataCollectingDialog.Max)
                {
                    //progressBar.Progress = samplesGathered;
                    Thread.Sleep(100);
                }
                RunOnUiThread(() => dataCollectingDialog.Dismiss());
            })).Start();*/
        }

        private void UpdateProgressDialog(int progress)
        {
            dataCollectingDialog.Progress = progress;
        }

        private void DismissProgressDialog()
        {
            dataCollectingDialog.Dismiss();
        }
        #endregion

        #region Handlers
        private async void OnFrontButtonClicked(object sender, EventArgs e)
        {
            await CollectData(PhoneOrientation.OnFront);
            frontCalibrated = true;
            frontButton.SetBackgroundResource(Resource.Drawable.GreenButton);
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await CollectData(PhoneOrientation.OnBack);
            backCalibrated = true;
            backButton.SetBackgroundResource(Resource.Drawable.GreenButton);
        }

        private async void OnTopButtonClicked(object sender, EventArgs e)
        {
            await CollectData(PhoneOrientation.OnTop);
            topCalibrated = true;
            topButton.SetBackgroundResource(Resource.Drawable.GreenButton);
        }

        private async void OnBottomButtonClicked(object sender, EventArgs e)
        {
            await CollectData(PhoneOrientation.OnBottom);
            topCalibrated = true;
            bottomButton.SetBackgroundResource(Resource.Drawable.GreenButton);
        }

        private async void OnLeftButtonClicked(object sender, EventArgs e)
        {
            await CollectData(PhoneOrientation.OnLeft);
            topCalibrated = true;
            leftButton.SetBackgroundResource(Resource.Drawable.GreenButton);
        }

        private async void OnRightButtonClicked(object sender, EventArgs e)
        {
            await CollectData(PhoneOrientation.OnRight);
            topCalibrated = true;
            rightButton.SetBackgroundResource(Resource.Drawable.GreenButton);
        }

        private void OnCalibrateButtonClicked(object sender, EventArgs e)
        {
            calibrateButton.SetBackgroundResource(Resource.Drawable.WhiteButton);
            Storage.AccelerometerCalibrationMatrix = R * r.Inversed();
         // calibrateTextView.Text = Storage.AccelerometerCalibrationMatrix.ToString();
        }
        #endregion
    }
}