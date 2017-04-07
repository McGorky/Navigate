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
        private EditText inputEditText;

        private Button calibrateMenuButton;
        private Button filterButton;
        private Button logButton;
        private Button logPlayerButton;
        private Button playPlotsButton;

        private bool backButtonPressed = false;
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.MainMenu);

            AlertDialog.Builder startAlert = new AlertDialog.Builder(this);
            // REMARK KK: добавить в values/Strings, использовать через GetString(Resource.String.***)
            startAlert.SetTitle("Do calibrate?");
            startAlert.SetPositiveButton("YES", OnCalibrateMenuButtonClicked);
            startAlert.SetNegativeButton("NO", CancelAction);

            calibrateMenuButton = FindViewById<Button>(Resource.Id.CalibrateMenuButton);
            filterButton = FindViewById<Button>(Resource.Id.FilterSettingsButton);
            logButton = FindViewById<Button>(Resource.Id.LogButton);
            logPlayerButton = FindViewById<Button>(Resource.Id.LogPlayerButton);
            playPlotsButton = FindViewById<Button>(Resource.Id.PlayPlotsButton);

            accelerometerPlotView = FindViewById<PlotView>(Resource.Id.AccelerometerPlotView);
            gyroPlotView = FindViewById<PlotView>(Resource.Id.GyroPlotView);
            magnetometerPlotView = FindViewById<PlotView>(Resource.Id.MagnetometerPlotView);

            var t = 0.0f;
            timer = new Timer((o) =>
            {
                UpdatePlot(accelerometerPlotView, new float[] { random.Next(10) - 5, random.Next(10) - 5, random.Next(10) - 5 }, t);
                t += 0.05f;
            });
            timer.Change(1000, 50);


            plotsSpinner = FindViewById<Spinner>(Resource.Id.PlotsSpinner);

            plotsLayout = FindViewById<LinearLayout>(Resource.Id.PlotsLayout);
            
            accelerometerPlotView.Model = CreatePlotModel("Time", "s", "Accelerometer", "m/s^2");
            gyroPlotView.Model = CreatePlotModel("Time", "s", "Gyro", "rad/s");
            magnetometerPlotView.Model = CreatePlotModel("Time", "s", "Magnetometer", "μT");

            calibrateMenuButton.Click += OnCalibrateMenuButtonClicked;
            filterButton.Click += OnFilterButtonClicked;
            logButton.Click += OnLogButtonClicked;
            logPlayerButton.Click += OnLogPlayerButtonClicked;
            playPlotsButton.Click += OnPlayPlotsButtonClicked;
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
                    dt = (data[0] - tPrev) * 0.001f;
                    tPrev = data[0];
                    // FIXME: данные должны нормироваться
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
                        var localRotation = Quaternion.CalculateDifference(startRotation, q);
                        aLinear = (0, data[10], data[11], data[12]);
                        var aGlobal = localRotation * aLinear * localRotation.Conjugated();
                        var accelerationCurrent = Filter.Exponential(acceleration, new float[] { aGlobal.X, aGlobal.Y, aGlobal.Z }, Storage.Gamma);
                        var velocityCurrent = Filter.Integrate(acceleration, accelerationCurrent, Storage.Velocity, dt);
                        offset = Filter.Integrate(velocity, velocityCurrent, offset, dt);
                        sw.WriteLine($"{dt * 1000},{q.ToString()},{offset[0]},{offset[1]},{offset[2]}");
                        //sw.WriteLine($"{dt * 1000},{q.ToString()},{0},{0},{0}");
                    }
                }
            }
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
            //plotsLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 50, 0f);
            //plotsSpinner.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 2f);
            var intent = new Intent(this, typeof(LogMenuActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
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

        void OnPlayPlotsButtonClicked(object sender, EventArgs e)
        {
            // plotsLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 50, 1f);
            // plotsSpinner.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 0f);
            var intent = new Intent(this, typeof(LogMenuActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }

        // REMARK KK: привести в порядок имена - <blabla>OkAction
        private void OkAction(object sender, DialogClickEventArgs e)
        {
        }

        private void CancelAction(object sender, DialogClickEventArgs e)
        {
        }
        #endregion        
    }
}