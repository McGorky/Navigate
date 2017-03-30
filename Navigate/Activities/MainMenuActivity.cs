using System;
using System.Timers;

using Xamarin.Android;

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

        PlotView accelerometerPlotView;
        PlotView gyroPlotView;
        PlotView magnetometerPlotView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            StartService(new Intent(this, typeof(SensorsDataService)));
            SetContentView(Resource.Layout.MainMenu);

            Button calibrateButton = FindViewById<Button>(Resource.Id.CalibrateButton);
            Button filterButton = FindViewById<Button>(Resource.Id.FilterSettingsButton);
            Button logButton = FindViewById<Button>(Resource.Id.LogButton);
            Button logPlayerButton = FindViewById<Button>(Resource.Id.ShowLogPlayerButton);
            Button clearButton = FindViewById<Button>(Resource.Id.ClearLogButton);

            accelerometerPlotView = FindViewById<PlotView>(Resource.Id.AccelerometerPlotView);
            gyroPlotView = FindViewById<PlotView>(Resource.Id.GyroPlotView);
            magnetometerPlotView = FindViewById<PlotView>(Resource.Id.MagnetometerPlotView);

            accelerometerPlotView.Model = CreatePlotModel("Time", "s", "Accelerometer", "m/s^2");
            gyroPlotView.Model = CreatePlotModel("Time", "s", "Gyro", "rad/s");
            magnetometerPlotView.Model = CreatePlotModel("Time", "s", "Magnetometer", "μT");

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

            // TODO: разобраться с лагами
            timer.Elapsed += (o, e) => RunOnUiThread(() => UpdatePlots());
            timer.Interval = 100;
            timer.Enabled = true;
        }

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

        private PlotModel CreatePlotModel(string xName, string xUnits, string yName, string yUnits)
        {
            var plotModel = new PlotModel();
            double fontSize = 7;

            var timeAxis = new LinearAxis { Position = AxisPosition.Bottom, FontSize = fontSize, Title = $"{xName}, {xUnits}" };
            var valueAxis = new LinearAxis { Position = AxisPosition.Left, FontSize = fontSize, Title = $"{yName}, {yUnits}" };

            plotModel.Axes.Add(timeAxis);
            plotModel.Axes.Add(valueAxis);

            var seriesX = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Red};
            var seriesY = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Green };
            var seriesZ = new LineSeries { MarkerType = MarkerType.None, Background = OxyColors.White, Color = OxyColors.Blue };

            plotModel.Series.Add(seriesX);
            plotModel.Series.Add(seriesY);
            plotModel.Series.Add(seriesZ);

            return plotModel;
        }
    }
}