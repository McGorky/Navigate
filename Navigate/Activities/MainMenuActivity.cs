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

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "@string/ApplicationName",
            MainLauncher = true,
            Icon = "@drawable/ApplicationIcon",
            Theme = "@style/DarkRedAndPink"
            )]
    public class MainMenuActivity : Activity
    {
        private Timer timer;
        private Random random = new Random(1);

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

            calibrateMenuButton = FindViewById<Button>(Resource.Id.CalibrateMenuButton);
            filterButton = FindViewById<Button>(Resource.Id.FilterSettingsButton);
            logButton = FindViewById<Button>(Resource.Id.LogButton);
            recordsButton = FindViewById<Button>(Resource.Id.RecordsButton);

            accelerometerPlotView = FindViewById<PlotView>(Resource.Id.AccelerometerPlotView);
            gyroPlotView = FindViewById<PlotView>(Resource.Id.GyroPlotView);
            magnetometerPlotView = FindViewById<PlotView>(Resource.Id.MagnetometerPlotView);

            var t = 0.0f;
            timer = new Timer((o) =>
            {
                UpdatePlot(accelerometerPlotView, new float[] { random.Next(10) - 5, random.Next(10) - 5, random.Next(10) - 5 }, t);
                t += 0.05f;
            });
            timer.Change(2000, 50);
            
            accelerometerPlotView.Model = CreatePlotModel("Time", "s", "Accelerometer", "m/s^2");
            gyroPlotView.Model = CreatePlotModel("Time", "s", "Gyro", "rad/s");
            magnetometerPlotView.Model = CreatePlotModel("Time", "s", "Magnetometer", "μT");

            calibrateMenuButton.Click += OnCalibrateMenuButtonClicked;
            filterButton.Click += OnFilterButtonClicked;
            logButton.Click += OnLogButtonClicked;
            recordsButton.Click += OnRecordsButtonClicked;
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
            if (backButtonPressed)
            {
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

        private void UpdatePlots()
        {
            var x = Storage.Uptime.TotalSeconds;

            for (int i = 0; i < 3; i++)
            {
                (accelerometerPlotView.Model.Series[i] as LineSeries).Points.Add(new DataPoint(x, Storage.AccelerometerData[i]));
                (gyroPlotView.Model.Series[i] as LineSeries).Points.Add(new DataPoint(x, Storage.GyroscopeData[i]));
                (magnetometerPlotView.Model.Series[i] as LineSeries).Points.Add(new DataPoint(x, Storage.MagnetometerData[i]));
            }

            accelerometerPlotView.Model.Axes[0].Minimum = x - 10;
            accelerometerPlotView.Model.Axes[0].Maximum = x + 5;
            accelerometerPlotView.Model.Axes[1].Minimum = -20;
            accelerometerPlotView.Model.Axes[1].Maximum = 20;

            gyroPlotView.Model.Axes[0].Minimum = x - 10;
            gyroPlotView.Model.Axes[0].Maximum = x + 5;
            gyroPlotView.Model.Axes[1].Minimum = -7;
            gyroPlotView.Model.Axes[1].Maximum = 7;

            magnetometerPlotView.Model.Axes[0].Minimum = x - 10;
            magnetometerPlotView.Model.Axes[0].Maximum = x + 5;
            magnetometerPlotView.Model.Axes[1].Minimum = -40;
            magnetometerPlotView.Model.Axes[1].Maximum = 40;

            accelerometerPlotView.InvalidatePlot();
            gyroPlotView.InvalidatePlot();
            magnetometerPlotView.InvalidatePlot();
        }

        private void UpdatePlot(PlotView plotView, float[] data, float t)
        {
            lock (Storage.DataAccessSync)
            {
                for (int i = 0; i < 3; i++)
                {
                    var s = plotView.Model.Series[i] as LineSeries;
                    s.Points.Add(new DataPoint(t, data[i]));
                    if (s.Points.Count == 150)
                        s.Points.RemoveAt(0);
                }

                plotView.Model.Axes[0].Minimum = t - 10;
                plotView.Model.Axes[0].Maximum = t + 3;
                plotView.Model.Axes[1].Minimum = -6;
                plotView.Model.Axes[1].Maximum = 5;
                RunOnUiThread(() => plotView.InvalidatePlot());
            }
        }

        private PlotModel CreatePlotModel(string xName, string xUnits, string yName, string yUnits)
        {
            var plotModel = new PlotModel();
            double fontSize = 7;

            var timeAxis = new LinearAxis { Position = AxisPosition.Bottom, FontSize = fontSize, Title = $"{xName}, {xUnits}", MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };
            var valueAxis = new LinearAxis { Position = AxisPosition.Left, FontSize = fontSize, Title = $"{yName}, {yUnits}", MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };

            plotModel.Axes.Add(timeAxis);
            plotModel.Axes.Add(valueAxis);

            var seriesX = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Red, MarkerSize = 0.5};
            var seriesY = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Green, MarkerSize = 0.5 };
            var seriesZ = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Blue, MarkerSize = 0.5 };

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
                saveLogInputEditText.Text = DateTime.Now.ToString("dd.MM.yyyy-HH:mm:ss");
                builder.SetPositiveButton(GetString(Resource.String.OkAction), SaveOkAction);
                builder.SetNegativeButton(GetString(Resource.String.CancelAction), SaveCancelAction);
                builder.Show();
            }
            else
            {
                StopService(new Intent(this, typeof(SensorsDataService)));
                logButton.Text = GetString(Resource.String.StartLog);
                isLogging = false;
                Toast.MakeText(this, $"{Storage.CurrentRawFile} saved", ToastLength.Short).Show();
            }

        }

        private void SaveOkAction(object sender, DialogClickEventArgs e)
        {
            Storage.Filename = saveLogInputEditText.Text + ".txt";
            logButton.Text = GetString(Resource.String.StopLog);
            isLogging = true;
            StartService(new Intent(this, typeof(SensorsDataService)));
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