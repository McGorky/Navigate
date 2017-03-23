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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainMenu);

            Button calibrateButton = ButtonBuilder.Create(this, Resource.Id.CalibrateButton);
            Button filterButton = ButtonBuilder.Create(this, Resource.Id.FilterSettingsButton);
            Button logButton = ButtonBuilder.Create(this, Resource.Id.LogButton);
            Button logPlayerButton = ButtonBuilder.Create(this, Resource.Id.ShowLogPlayerButton);
            Button clearButton = ButtonBuilder.Create(this, Resource.Id.ClearLogButton);

            PlotView accelerometerPlotView = FindViewById<PlotView>(Resource.Id.AccelerometerPlotView);
            PlotView gyroPlotView = FindViewById<PlotView>(Resource.Id.GyroPlotView);
            PlotView magnetometerPlotView = FindViewById<PlotView>(Resource.Id.MagnetometerPlotView);
            accelerometerPlotView.Model = CreatePlotModel("Time", "s", "Accelerometer", "m/s^2");
            gyroPlotView.Model = CreatePlotModel("Time", "s", "Gyro", "rad/s");
            magnetometerPlotView.Model = CreatePlotModel("Time", "s", "Magnetometer", "T");


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
    }
}