using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Android;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;
using Android.Graphics;

using OpenTK;

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using Mirea.Snar2017.Navigate.Services;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "@string/ApplicationName",
            MainLauncher = true,
            Icon = "@drawable/ApplicationIcon",
            Theme = "@style/DarkRedAndPink"
            )]
    public class MainMenuActivity : Activity
    {
        private System.Timers.Timer timer;

        #region Views and related fields
        private PlotView accelerometerPlotView;
        private PlotView gyroPlotView;
        private PlotView magnetometerPlotView;
        private LinearLayout plotsLayout;
        private Spinner plotsSpinner;
        private EditText saveLogInputEditText;

        private Button calibrateMenuButton;
        private Button filterButton;
        private Button logButton;
        private Button recordsButton;
        private Button realtimeButton;

        private bool isLogging = false;
        private bool backButtonPressed = false;
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.MainMenu);

            var startAlert = new AlertDialog.Builder(this);
            startAlert.SetTitle(GetString(Resource.String.AlertCalibrateStart));
            startAlert.SetPositiveButton(GetString(Resource.String.OkAction), OnCalibrateMenuButtonClicked);
            startAlert.SetNegativeButton(GetString(Resource.String.CancelAction), SaveCancelAction);
            //startAlert.Show();

            calibrateMenuButton = FindViewById<Button>(Resource.Id.CalibrateMenuButton);
            filterButton = FindViewById<Button>(Resource.Id.FilterSettingsButton);
            logButton = FindViewById<Button>(Resource.Id.LogButton);
            recordsButton = FindViewById<Button>(Resource.Id.RecordsButton);
            realtimeButton = FindViewById<Button>(Resource.Id.RealTime3dButton);

            accelerometerPlotView = FindViewById<PlotView>(Resource.Id.AccelerometerPlotView);
            gyroPlotView = FindViewById<PlotView>(Resource.Id.GyroPlotView);
            magnetometerPlotView = FindViewById<PlotView>(Resource.Id.MagnetometerPlotView);

            Storage.Current.SensorsDataServiceConnected += (object sender, ServiceConnectedEventArgs e) =>
            {
                
                Storage.Current.SensorsDataService.SensorsReadingsUpdated += OnSensorReadingsUpdated; // обновить графики
                Storage.Current.SensorsDataService.StateVectorUpdated += delegate { }; // обновить opengl
            };
            Storage.StartSensorsDataService();

            accelerometerPlotView.Model = CreatePlotModel("Time", "s", "Accelerometer", "m/s^2", 5, 20);
            gyroPlotView.Model = CreatePlotModel("Time", "s", "Gyro", "rad/s", 5, 7);
            magnetometerPlotView.Model = CreatePlotModel("Time", "s", "Magnetometer", "μT", 10, 55);

            calibrateMenuButton.Click += OnCalibrateMenuButtonClicked;
            filterButton.Click += OnFilterButtonClicked;
            logButton.Click += OnLogButtonClicked;
            recordsButton.Click += OnRecordsButtonClicked;

            //timer.Elapsed += (o, e) => { };
            //timer.AutoReset = false;
            //timer.Interval = 1000;
            //timer.Start();
            realtimeButton.Click += OnRealTime3dButtonClicked;
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
            if (backButtonPressed)
            {
                Storage.Current.SaveSettings(this);
                base.OnBackPressed();
                Process.KillProcess(Process.MyPid());
            }
            else
            {
                backButtonPressed = true;
                Toast.MakeText(this, GetString(Resource.String.PressBackToExit), ToastLength.Long).Show();
                Task.Run(async () =>
                {
                    await Task.Delay(3500);
                    backButtonPressed = false;
                });
            }
        }
        #endregion

        //int counter = 0;
        private void OnSensorReadingsUpdated(object sender, SensorsReadingsUpdatedEventArgs e)
        {
            //counter++;
            //if (counter < 10)
            //    return;
            //counter = 0;

            var t = e.Value.Time;
            var accelerometerData = new double[] { e.Value.Acceleration.X, e.Value.Acceleration.Y, e.Value.Acceleration.Z };
            var gyroscopeData = new double[] { e.Value.AngularVelocity.X, e.Value.AngularVelocity.Y, e.Value.AngularVelocity.Z };
            var magnetometerData = new double[] { e.Value.MagneticField.X, e.Value.MagneticField.Y, e.Value.MagneticField.Z };

            for (int i = 0; i < 3; i++)
            {
                var accelerometerSeries = accelerometerPlotView.Model.Series[i] as LineSeries;
                var gyroscopeSeries = gyroPlotView.Model.Series[i] as LineSeries;
                var magnetometerSeries = magnetometerPlotView.Model.Series[i] as LineSeries;

                if (accelerometerSeries.Points.Count >= accelerometerSeries.Points.Capacity - 1)
                {
                    accelerometerSeries.Points.RemoveAt(0);
                    gyroscopeSeries.Points.RemoveAt(0);
                    magnetometerSeries.Points.RemoveAt(0);
                }
                accelerometerSeries.Points.Add(new DataPoint(t, accelerometerData[i]));
                gyroscopeSeries.Points.Add(new DataPoint(t, gyroscopeData[i]));
                magnetometerSeries.Points.Add(new DataPoint(t, magnetometerData[i]));
            }

            accelerometerPlotView.Model.Axes[0].Minimum = t - 5;
            accelerometerPlotView.Model.Axes[0].Maximum = t + 2;

            gyroPlotView.Model.Axes[0].Minimum = t - 5;
            gyroPlotView.Model.Axes[0].Maximum = t + 2;

            magnetometerPlotView.Model.Axes[0].Minimum = t - 5;
            magnetometerPlotView.Model.Axes[0].Maximum = t + 2;

            accelerometerPlotView.InvalidatePlot();
            gyroPlotView.InvalidatePlot();
            magnetometerPlotView.InvalidatePlot();
        }

        private PlotModel CreatePlotModel(string xName, string xUnits, string yName, string yUnits, double yStep, double yMax)
        {
            var plotModel = new PlotModel();
            double fontSize = 7;

            var timeAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                FontSize = fontSize,
                Title = $"{xName}, {xUnits}",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineThickness = 0.1
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                FontSize = fontSize,
                Title = $"{yName}, {yUnits}",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineThickness = 0.1,
                MajorStep = yStep,
                Maximum = yMax,
                Minimum = -yMax
            };

            plotModel.Axes.Add(timeAxis);
            plotModel.Axes.Add(valueAxis);

            var seriesX = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Red };
            var seriesY = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Green };
            var seriesZ = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Blue };

            seriesX.Points.Capacity = 250;
            seriesY.Points.Capacity = 250;
            seriesZ.Points.Capacity = 250;

            plotModel.Series.Add(seriesX);
            plotModel.Series.Add(seriesY);
            plotModel.Series.Add(seriesZ);

            return plotModel;
        }

        #region Handlers
        void OnCalibrateMenuButtonClicked(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(CalibrateMenuActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }

        void OnRealTime3dButtonClicked(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(RealTime3dActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }

        void OnFilterButtonClicked(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(FilterSettingsActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }

        void OnLogButtonClicked(object sender, EventArgs e)
        {
            if(!isLogging)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle("Set log name");
                saveLogInputEditText = new EditText(this);
                builder.SetView(saveLogInputEditText);
                saveLogInputEditText.Text = DateTime.Now.ToString("dd.MM.yyyy-HH.mm.ss");
                builder.SetPositiveButton(GetString(Resource.String.OkAction), SaveOkAction);
                builder.SetNegativeButton(GetString(Resource.String.CancelAction), SaveCancelAction);
                builder.Show();
                isLogging = true;
            }
            else
            {
                //StopService(new Intent(this, typeof(SensorsDataService)));
                logButton.Text = GetString(Resource.String.StartLog);
                isLogging = false;
                //Toast.MakeText(this, $"{Storage.CurrentRawFile} saved", ToastLength.Short).Show();
            }

        }

        private void SaveOkAction(object sender, DialogClickEventArgs e)
        {
            Storage.CurrentFilename = saveLogInputEditText.Text + ".txt";
            logButton.Text = GetString(Resource.String.StopLog);
            isLogging = true;
        }

        private void SaveCancelAction(object sender, DialogClickEventArgs e)
        {

        }

        void OnLogPlayerButtonClicked(object sender, EventArgs e)
        {
            /*using (var sr = new StreamReader(Storage.CurrentFile))
            {
                var line = sr.ReadLine();
                Storage.numberOfFrames = int.Parse(line, CultureInfo.InvariantCulture);
                Storage.data = new float[Storage.numberOfFrames][];
                var text = sr.ReadToEnd().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Storage.numberOfFrames; i++)
                {
                    Storage.data[i] = new float[8];
                    var s = text[i].Split(new char[] { ',' });
                    for (int j = 0; j < 8; j++)
                    {
                        Storage.data[i][j] = float.Parse(s[j], CultureInfo.InvariantCulture);
                    }
                }
            }
            */
            var intent = new Intent(this, typeof(LogPlayerActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }

        void OnRecordsButtonClicked(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(RecordsMenuActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }
        #endregion        
    }
}