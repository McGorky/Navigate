﻿using System;
using System.Timers;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "@string/ApplicationName",
            MainLauncher = true,
            Icon = "@drawable/ApplicationIcon",
            Theme = "@style/DarkAndGray"
            )]
    public class MainMenuActivity : Activity
    {
        private Timer timer = new Timer();
        private Random random = new Random(1);

        Button logButton;
        int step;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.MainMenu);

            Button calibrateButton = FindViewById<Button>(Resource.Id.CalibrateButton);
            Button filterButton = FindViewById<Button>(Resource.Id.FilterSettingsButton);
            logButton = FindViewById<Button>(Resource.Id.LogButton);
            Button logPlayerButton = FindViewById<Button>(Resource.Id.ShowLogPlayerButton);
            Button clearButton = FindViewById<Button>(Resource.Id.ClearLogButton);

            PlotView accelerometerPlotView = FindViewById<PlotView>(Resource.Id.AccelerometerPlotView);
            PlotView gyroPlotView = FindViewById<PlotView>(Resource.Id.GyroPlotView);
            PlotView magnetometerPlotView = FindViewById<PlotView>(Resource.Id.MagnetometerPlotView);
            accelerometerPlotView.Model = CreatePlotModel("Time", "s", "Accelerometer", "m/s^2");
            gyroPlotView.Model = CreatePlotModel("Time", "s", "Gyro", "rad/s");
            magnetometerPlotView.Model = CreatePlotModel("Time", "s", "Magnetometer", "T");

            logButton.Click += logButtonClicked;
            clearButton.Click += SaveButtonClicked;


            logPlayerButton.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        {
                            break;
                        }
                    case MotionEventActions.Up:
                        {
                            var intent = new Intent(this, typeof(LogPlayerActivity));
                            StartActivity(intent);
                            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
                            break;
                        }
                }
            };

            calibrateButton.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                    {
                        break;
                    }
                    case MotionEventActions.Up:
                    {
                        var intent = new Intent(this, typeof(CalibrateMenuActivity));
                        StartActivity(intent);
                        OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
                        break;
                    }
                }
            };

            filterButton.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                        {
                            break;
                        }
                    case MotionEventActions.Up:
                        {
                            var intent = new Intent(this, typeof(FilterSetsActivity));
                            StartActivity(intent);
                            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
                            break;
                        }
                }
            };


            double time = 0;
            double x = 0;
            timer.Elapsed += (o, e) =>
            {
                RunOnUiThread(() =>
                {
                    time += timer.Interval;
                    x += 1;
                    var y1 = random.NextDouble() * random.Next(0, 7);
                    var y2 = random.NextDouble() * random.Next(0, 7);
                    var y3 = random.NextDouble() * random.Next(0, 7);

                    (accelerometerPlotView.Model.Series[0] as LineSeries).Points.Add(new DataPoint(x, y1));
                    accelerometerPlotView.Model.Axes[0].Minimum = x - 7;
                    accelerometerPlotView.Model.Axes[0].Maximum = x + 3;
                    accelerometerPlotView.InvalidatePlot();

                    (gyroPlotView.Model.Series[0] as LineSeries).Points.Add(new DataPoint(x, y2));
                    gyroPlotView.Model.Axes[0].Minimum = x - 7;
                    gyroPlotView.Model.Axes[0].Maximum = x + 3;
                    gyroPlotView.InvalidatePlot();

                    (magnetometerPlotView.Model.Series[0] as LineSeries).Points.Add(new DataPoint(x, y3));
                    magnetometerPlotView.Model.Axes[0].Minimum = x - 7;
                    magnetometerPlotView.Model.Axes[0].Maximum = x + 3;
                    magnetometerPlotView.InvalidatePlot();
                });
            };
            timer.Interval = 100;
            timer.Enabled = true;
        }

        private PlotModel CreatePlotModel(string xName, string xUnits, string yName, string yUnits)
        {
            var plotModel = new PlotModel();
            double fontSize = 7;

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, FontSize = fontSize, Title = $"{xName}, {xUnits}" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 10, Minimum = 0, FontSize = fontSize, Title = $"{yName}, {yUnits}"});

            var series = new LineSeries
            {
                MarkerType = MarkerType.None,
            };
            series.Background = OxyColors.White;

            plotModel.Series.Add(series);

            return plotModel;
        }

        void SaveButtonClicked(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Name");
            EditText inp = new EditText(this);
            builder.SetView(inp);
            inp.RequestFocus();

            builder.SetPositiveButton("OK", OkAction);
            builder.SetNegativeButton("Cancel", CancelAction);
            builder.Show();

            
            Android.Views.InputMethods.InputMethodManager inputManager = (Android.Views.InputMethods.InputMethodManager)GetSystemService(MainMenuActivity.InputMethodService);
            inputManager.ShowSoftInput(inp, Android.Views.InputMethods.ShowFlags.Implicit);
            inputManager.ToggleSoftInput(Android.Views.InputMethods.ShowFlags.Forced, Android.Views.InputMethods.HideSoftInputFlags.ImplicitOnly);
                        
        }

        private void OkAction(object sender, DialogClickEventArgs e)
        {
            
        }

        private void CancelAction(object sender, DialogClickEventArgs e)
        {

        }



        void logButtonClicked(object sender, EventArgs e)
        {
            if (logButton.Text == "Start Log")
            {
                logButton.Text = "Stop";

                ProgressDialog progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetMessage("Loading...");
                progressBar.SetCancelable(false);
                progressBar.Progress = 0;
                progressBar.Max = 0;
                
                progressBar.Show();
                step = 0;
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    while (step < 100)
                    {
                        step += 1;
                        progressBar.Progress = step;
                        System.Threading.Thread.Sleep(100);
                    }
                    RunOnUiThread(() => { progressBar.Dismiss(); });
                })).Start();
            }
            else
            {
                logButton.Text = "Start Log";
            }
        }
    }
}