using System;
using System.Timers;
using System.IO;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

//using Xamarin.Forms;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;
using Android.Text;
using Android.Media;

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "Filter Settings",
        Theme = "@style/DarkRedAndPink")]
    public class RecordsMenuActivity : Activity
    {
        #region Views and related fields
        private Button showPlotsButton;
        private Button show3DButton;

        private Spinner logTypeSpinner;
        private Spinner logSelectSpinner;

        private Switch drawTrajectorySwitch;

        private bool playFromRawData = true;
        private List<string> LogTypeOptions = new List<string>() { "Raw data" };
        private List<string> SavedLogs
        {
            get
            {
                string path;
                if (playFromRawData)
                    path = Storage.RawFolderName;
                else
                    path = Storage.FilteredFolderName;

                var result = Directory.GetFiles(path);
                

                return result.Select(f => Path.GetFileName(f)).ToList();
            }
        }
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.RecordsMenu);

            logTypeSpinner = FindViewById<Spinner>(Resource.Id.LogTypeSpinner);
            logSelectSpinner = FindViewById<Spinner>(Resource.Id.LogNameSpinner);

            showPlotsButton = FindViewById<Button>(Resource.Id.ShowPlotsButton);
            show3DButton = FindViewById<Button>(Resource.Id.Show3DButton);

            drawTrajectorySwitch = FindViewById<Switch>(Resource.Id.DrawTrajectorySwitch);
            
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, LogTypeOptions);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            logTypeSpinner.Adapter = adapter;

            /*adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, SavedLogs);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            logSelectSpinner.Adapter = adapter;*/

            logTypeSpinner.ItemSelected += OnLogTypeSpinnerItemSelected;
            logSelectSpinner.ItemSelected += OnLogSelectSpinnerItemSelected;

            showPlotsButton.Click += OnShowPlotsButtonClicked;
            show3DButton.Click += OnShow3DButtonClicked;

            drawTrajectorySwitch.CheckedChange += OnDrawTrajectorySwitchCheckedChange;

            RunOnUiThread(() =>
            {
                drawTrajectorySwitch.Checked = Storage.TrajectoryTracingEnabled;
            });
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

        private void Compile(string fileName)
        {
            string[] text;
            DateTime startTime;
            var calibrationMatrix = new Matrix(4, 4);
            using (var sr = new StreamReader(fileName))
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
            string filteredFileName = Path.Combine(Storage.FilteredFolderName, $"from {Path.GetFileNameWithoutExtension(fileName)}.txt");
            Directory.CreateDirectory(Storage.FilteredFolderName);
            using (var sw = new StreamWriter(File.Open(filteredFileName, FileMode.Create, FileAccess.Write)))
            {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                Quaternion a = (0, 0, 0, 0.01f);
                Quaternion g = (0, 0, 0, 0.01f);
                Quaternion m = (0, 0, 0, 0.01f);
                Quaternion q = (1, 0, 0, 0);
                Quaternion qPrev = (1, 0, 0, 0);
                Quaternion startRotation = (1, 0, 0, 0);
                Quaternion aLinear;
                float dt;
                float tPrev = float.Parse(text[0].Split(new char[] { ',' })[0], CultureInfo.InvariantCulture);
                float beta = 2.0f;
                bool converged = false;

                var acceleration = new float[3];
                var velocity = new float[3];
                var offset = new float[3];

                /*Storage.Accelerometer = new float[text.Length - 1][];
                Storage.Gyro = new float[text.Length - 1][];
                Storage.Magnetometer = new float[text.Length - 1][];

                Storage.Phi = new float[text.Length - 1];
                Storage.Theta = new float[text.Length - 1];
                Storage.Psi = new float[text.Length - 1];*/

                int startOffsetIndex = 0;
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

                    a = new Quaternion(0, data[1], data[2], data[3]);
                    a = Filter.Calibrate(calibrationMatrix, a);
                    g = new Quaternion(0, data[4], data[5], data[6]);
                    m = (new Quaternion(0, data[7], data[8], data[9]) * 0.001f);

                    /*Storage.Accelerometer[i - 1] = new float[] { data[1], data[2], data[3] };
                    Storage.Gyro[i - 1] = new float[] { data[4], data[5], data[6] };
                    Storage.Magnetometer[i - 1] = new float[] { data[7], data[8], data[9] };*/

                    if (!converged && data[0] > 2000)
                    {
                        startRotation = q;
                        beta = Storage.Beta;
                        converged = true;

                        acceleration = new float[3];
                        velocity = new float[3];
                        offset = new float[3];

                        startOffsetIndex = i;
                        //Storage.Offsets = new float[text.Length - i][];
                        sw.WriteLine($"{text.Length - i}");
                    }
                    q = Filter.Madgvic(q, g, a, m, beta, Storage.Zeta, dt);

                    if (i > 15)
                    {
                        var angles = Storage.ToEulerAngles(q.Normalized());
                        Storage.Phi.Points.Add(new DataPoint(data[0] / 1000, angles.Phi * 180 / 3.1416f));
                        Storage.Theta.Points.Add(new DataPoint(data[0] / 1000, angles.Theta * 180 / 3.1416f));
                        Storage.Psi.Points.Add(new DataPoint(data[0] / 1000, angles.Psi * 180 / 3.1416f));
                    }

                    var localRotation = Quaternion.CalculateDifference(startRotation, q).Normalized();
                    aLinear = (0, data[10], data[11], data[12]);
                    var aGlobal = localRotation * aLinear * localRotation.Conjugated();

                    var accelerationCurrent = Filter.Exponential(acceleration, new float[] { aGlobal.X, aGlobal.Y, aGlobal.Z }, Storage.Gamma);
                    var velocityCurrent = Filter.Integrate(acceleration, accelerationCurrent, velocity, dt);
                    offset = Filter.Integrate(velocity, velocityCurrent, offset, dt);

                    acceleration = accelerationCurrent;
                    velocity = velocityCurrent;

                    if (converged)
                    {
                        //Storage.Offsets[i - startOffsetIndex] = new float[] { offset[0], offset[1], offset[2] };
                        sw.WriteLine($"{(int)Math.Round(dt * 1000, MidpointRounding.AwayFromZero)},{q.ToString()},{offset[0]},{offset[1]},{offset[2]}");
                    }
                }
            }

            using (var fs = new FileStream(filteredFileName, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var line = sr.ReadLine();
                    Storage.NumberOfFrames = int.Parse(line, CultureInfo.InvariantCulture);
                    Storage.Data = new float[Storage.NumberOfFrames][];
                    var tmp = sr.ReadToEnd().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < Storage.NumberOfFrames; i++)
                    {
                        Storage.Data[i] = new float[8];
                        var s = tmp[i].Split(new char[] { ',' });
                        for (int j = 0; j < 8; j++)
                        {
                            Storage.Data[i][j] = float.Parse(s[j], CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            MediaScannerConnection.ScanFile(this, new string[] { Storage.FilteredFolderName }, null, null);
        }

        #region Handlers
        private void OnLogTypeSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var spinner = sender as Spinner;
            string text = $"{spinner.GetItemAtPosition(e.Position)}";
            //Toast.MakeText(this, text, ToastLength.Short).Show();

            switch (text)
            {
                case "Raw data":
                {
                    playFromRawData = true;
                    break;
                }
                case "Filtered data":
                {
                    playFromRawData = false;
                    break;
                }
            }

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, SavedLogs);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            logSelectSpinner.Adapter = adapter;
        }
        private void OnLogSelectSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var spinner = sender as Spinner;
            string fileName = $"{spinner.GetItemAtPosition(e.Position)}";
            Toast.MakeText(this, fileName, ToastLength.Short).Show();
            string fullFileName;
            if (playFromRawData)
                fullFileName = Path.Combine(Storage.RawFolderName, fileName);
            else
                fullFileName = Path.Combine(Storage.FilteredFolderName, fileName);

            Storage.Phi.Points.Clear();
            Storage.Psi.Points.Clear();
            Storage.Theta.Points.Clear();
            Compile(fullFileName);
        }

        void OnShowPlotsButtonClicked(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(LogMenuActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }

        void OnShow3DButtonClicked(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(LogPlayerActivity));
            StartActivity(intent);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }

        private void OnDrawTrajectorySwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.TrajectoryTracingEnabled = drawTrajectorySwitch.Checked;
        }
        #endregion
    }
}  
