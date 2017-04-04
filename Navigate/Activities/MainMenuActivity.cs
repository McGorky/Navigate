using System;
using System.Globalization;
using System.Text;
using System.IO;
using System.Threading;
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
using Android.Graphics;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "@string/ApplicationName",
            MainLauncher = true,
            Icon = "@drawable/ApplicationIcon",
            Theme = "@style/DarkAndGray"
            )]
    public class MainMenuActivity : Activity
    {
        private System.Timers.Timer timer = new System.Timers.Timer();

        PlotView accelerometerPlotView;
        PlotView gyroPlotView;
        PlotView magnetometerPlotView;
        LinearLayout plotsLayout;
        Spinner plotsSpinner;

        Button calibrateMenuButton;
        Button filterButton;
        Button logButton;
        Button logPlayerButton;
        Button playPlotsButton;
        int step;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.MainMenu);

            AlertDialog.Builder startAlert = new AlertDialog.Builder(this);
            startAlert.SetTitle("Do calibrate?");
            startAlert.SetPositiveButton("YES", calibrateMenuButton_Clicked);
            startAlert.SetNegativeButton("NO", CancelAction);

            calibrateMenuButton = FindViewById<Button>(Resource.Id.CalibrateMenuButton);
            filterButton = FindViewById<Button>(Resource.Id.FilterSettingsButton);
            logButton = FindViewById<Button>(Resource.Id.LogButton);
            logPlayerButton = FindViewById<Button>(Resource.Id.LogPlayerButton);
            playPlotsButton = FindViewById<Button>(Resource.Id.PlayPlotsButton);

            accelerometerPlotView = FindViewById<PlotView>(Resource.Id.AccelerometerPlotView);
            gyroPlotView = FindViewById<PlotView>(Resource.Id.GyroPlotView);
            magnetometerPlotView = FindViewById<PlotView>(Resource.Id.MagnetometerPlotView);

            plotsSpinner = FindViewById<Spinner>(Resource.Id.PlotsSpinner);

            plotsLayout = FindViewById<LinearLayout>(Resource.Id.PlotsLayout);
            
            accelerometerPlotView.Model = CreatePlotModel("Time", "s", "Accelerometer", "m/s^2");
            gyroPlotView.Model = CreatePlotModel("Time", "s", "Gyro", "rad/s");
            magnetometerPlotView.Model = CreatePlotModel("Time", "s", "Magnetometer", "μT");

            calibrateMenuButton.Click += calibrateMenuButton_Clicked;
            filterButton.Click += filterButton_Clicked;
            logButton.Click += logButton_Clicked;
            logPlayerButton.Click += logPlayerButton_Clicked;
            playPlotsButton.Click += playPlotsButton_Clicked;

            void calibrateMenuButton_Clicked(object sender, EventArgs e)
            {
                var intent = new Intent(this, typeof(CalibrateMenuActivity));
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
            }

            void filterButton_Clicked(object sender, EventArgs e)
            {
                var intent = new Intent(this, typeof(FilterSettingsActivity));
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
            }

            void logButton_Clicked(object sender, EventArgs e)
            {
                //plotsLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 50, 0f);
                //plotsSpinner.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 2f);
                var intent = new Intent(this, typeof(LogMenuActivity));
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
            }

            void logPlayerButton_Clicked(object sender, EventArgs e)
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

            

            void playPlotsButton_Clicked(object sender, EventArgs e)
            {
               // plotsLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 50, 1f);
               // plotsSpinner.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 0f);
                var intent = new Intent(this, typeof(LogMenuActivity));
                StartActivity(intent);
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
            }



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

        private EditText inp;
         /*void SaveButtonClicked(object sender, EventArgs e)
         {
             AlertDialog.Builder builder = new AlertDialog.Builder(this);
             builder.SetTitle("Name");
             inp = new EditText(this);
             builder.SetView(inp);
             inp.RequestFocus();

             builder.SetPositiveButton("OK", OkAction);
             builder.SetNegativeButton("Cancel", CancelAction);
             builder.Show();
         }
         */

        private void CancelAction(object sender, DialogClickEventArgs e)
        {
        }

        private void Compile(string fileName)
        {
            string[] text;
            DateTime startTime;
            var calibrationMatrix = new Matrix(4, 4);
            using (var sr = new StreamReader(Storage.Filename))
            {
                for (int i = 0; i < 4; i++)
                {
                    var s = sr.ReadLine().Split(new char[] { ',' });
                    for (int j = 0; j < 4; j++)
                    {
                        calibrationMatrix[i, j] = float.Parse(s[j], CultureInfo.InvariantCulture);
                    }
                }
                startTime = DateTime.Parse(sr.ReadLine(), CultureInfo.InvariantCulture);
                var tmp = sr.ReadToEnd();
                text = tmp.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            using (var sw = new StreamWriter(File.Open(System.IO.Path.Combine(Storage.Path, fileName), FileMode.Create, FileAccess.Write)))
            {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                Quaternion a = (0, 0, 0, 0.01f);
                Quaternion g = (0, 0, 0, 0.01f);
                Quaternion m = (0, 0, 0, 0.01f);
                Quaternion q = (1, 0, 0, 0);
                Quaternion startRotation = (1, 0, 0, 0);
                Quaternion aLinear;
                float dt;
                float tPrev = float.Parse(text[0].Split(new char[] { ',' })[0], CultureInfo.InvariantCulture);
                float beta = 0.9f;
                bool calibrated = false;

                var acceleration = new float[3];
                var velocity = new float[3];
                var offset = new float[3];

                for (int i = 1; i < text.Length; i++)
                {
                    var s = text[i].Split(new char[] { ',' });
                    var data = new float[13];
                    for (int j = 0; j < 13; j++)
                    {
                        data[j] = float.Parse(s[j], CultureInfo.InvariantCulture);
                    }
                    dt = (data[0] - tPrev)*0.001f;
                    tPrev = data[0];
                    a = (0, data[1], data[2], data[3]);
                    a = Filter.Calibrate(calibrationMatrix, a);
                    g = (0, data[4], data[5], data[6]);
                    m = new Quaternion(0, data[7], data[8], data[9]) * 0.001f;

                    if (!calibrated && data[0] > 9000)
                    {
                        beta = Storage.Beta;
                        startRotation = q;
                        calibrated = true;
                        sw.WriteLine($"{text.Length - i}");
                    }
                    q = Filter.Madgvic(q, g, a, m, beta, Storage.Zeta, dt);
                    if (calibrated)
                    {
                        var localRotation = Filter.CalculateDifferenceQuaternion(startRotation, q);
                        aLinear = (0, data[10], data[11], data[12]);
                        var aGlobal = localRotation * aLinear * localRotation.Conjugated();
                        var accelerationCurrent = Filter.Exponential(acceleration, new float[] { aGlobal.x, aGlobal.y, aGlobal.z }, Storage.Gamma);
                        var velocityCurrent = Filter.Integrate(acceleration, accelerationCurrent, Storage.Velocity, dt);
                        offset = Filter.Integrate(velocity, velocityCurrent, offset, dt);
                        sw.WriteLine($"{dt * 1000},{q.ToString()},{offset[0]},{offset[1]},{offset[2]}");
                        //sw.WriteLine($"{dt * 1000},{q.ToString()},{0},{0},{0}");
                    }
                }
            }
        }

        
    }
}