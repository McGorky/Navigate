using System;
using System.Timers;
using System.Threading;
using System.Globalization;

//using Xamarin.Forms;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;
using Android.Text;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "Filter Settings",
        Theme = "@style/DarkRedAndPink")]
    public class FilterSettingsActivity : Activity
    {
        #region Views and related fields
        private SeekBar betaSeekbar,
            zetaSeekbar,
            gammaSeekbar;

        private EditText betaEditText,
            zetaEditText,
            gammaEditText;

        private Switch magnetometerSwitch,
            gyroscopeDriftSwitch,
            accelerometerCalibrationSwitch,
            gyroscopeCalibrationSwitch;
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FilterSettings);

            betaSeekbar = FindViewById<SeekBar>(Resource.Id.BetaSeekBar);
            zetaSeekbar = FindViewById<SeekBar>(Resource.Id.ZetaSeekBar);
            gammaSeekbar = FindViewById<SeekBar>(Resource.Id.ExponentialSeekBar);

            // REMARK KK: привести в пор€док имена в axml
            betaEditText = FindViewById<EditText>(Resource.Id.MadgwickEditText1);
            zetaEditText = FindViewById<EditText>(Resource.Id.MadgwickEditText2);
            gammaEditText = FindViewById<EditText>(Resource.Id.ExponentialEditText);

            magnetometerSwitch = FindViewById<Switch>(Resource.Id.MagnetometerSwitch);
            gyroscopeDriftSwitch = FindViewById<Switch>(Resource.Id.GyroscopeDriftCompensationSwitch);
            accelerometerCalibrationSwitch = FindViewById<Switch>(Resource.Id.AccelerometerCalibrationSwitch);
            gyroscopeCalibrationSwitch = FindViewById<Switch>(Resource.Id.GyroscopeCalibrationSwitch);

            magnetometerSwitch.Enabled = false;
            gyroscopeDriftSwitch.Enabled = false;
            zetaSeekbar.Enabled = false;
            zetaEditText.Enabled = false;
            gyroscopeCalibrationSwitch.Enabled = false;

            betaSeekbar.ProgressChanged += OnBetaProgressChanged;
            zetaSeekbar.ProgressChanged += OnZetaProgressChanged;
            gammaSeekbar.ProgressChanged += OnGammaProgressChanged;

            magnetometerSwitch.CheckedChange += OnMagnetometerSwitchCheckedChange;
            gyroscopeDriftSwitch.CheckedChange += OnGyroscopeDriftSwitchCheckedChange;
            accelerometerCalibrationSwitch.CheckedChange += OnAccelerometerCalibrationSwitchCheckedChange;
            gyroscopeCalibrationSwitch.CheckedChange += OnGyroscopeCalibrationSwitchCheckedChange;

            RunOnUiThread(() =>
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                betaEditText.Text = $"{Storage.Current.Beta:F3}";
                zetaEditText.Text = $"{Storage.Current.Zeta:F3}";
                gammaEditText.Text = $"{Storage.Current.Gamma:F3}";

                betaSeekbar.Max = 1000;
                betaSeekbar.Progress = (int)(Storage.Current.Beta * 1000);
                betaSeekbar.RefreshDrawableState();

                zetaSeekbar.Max = 1000;
                zetaSeekbar.Progress = (int)(Storage.Current.Zeta * 1000);
                zetaSeekbar.RefreshDrawableState();

                gammaSeekbar.Max = 1000;
                gammaSeekbar.Progress = (int)(Storage.Current.Gamma * 1000);
                gammaSeekbar.RefreshDrawableState();

                magnetometerSwitch.Checked = Storage.Current.MagnetometerEnabled;
                gyroscopeDriftSwitch.Checked = Storage.Current.GyroscopeDriftCompensationEnabled;
                accelerometerCalibrationSwitch.Checked = Storage.Current.AccelerometerCalibrationEnabled;
                gyroscopeCalibrationSwitch.Checked = Storage.Current.GyroscopeCalibrationEnabled;
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

        // REMARK KK: заменить все string.Format на интерпол€цию строк
        #region Handlers
        void OnBetaProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                if (e.Progress == 0)
                {
                    Storage.Current.Beta = 0;
                    betaEditText.Text = $"{0.0f:F3}"; ;
                }
                else
                {
                    //Storage.Current.Beta = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    Storage.Current.Beta = e.Progress / 1000.0f;
                    betaEditText.Text = $"{Storage.Current.Beta:F3}";
                }
            }
        }

        void OnZetaProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                if (e.Progress == 0)
                {
                    Storage.Current.Zeta = 0;
                    zetaEditText.Text = string.Format("0.000");
                }
                else
                {
                    //Storage.Current.Zeta = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    Storage.Current.Zeta = e.Progress / 1000.0f;
                    zetaEditText.Text = string.Format("{0:F3}", Storage.Current.Zeta);
                }
            }
        }

        void OnGammaProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                if (e.Progress == 0)
                {
                    Storage.Current.Gamma = 0;
                    gammaEditText.Text = string.Format("0.000");
                }
                else
                {
                    //Storage.Current.Gamma = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    Storage.Current.Gamma = e.Progress / 1000.0f;
                    gammaEditText.Text = string.Format("{0:F3}", Storage.Current.Gamma);
                }
            }
        }

        private void OnMagnetometerSwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.Current.MagnetometerEnabled = e.IsChecked;
        }

        private void OnGyroscopeDriftSwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.Current.GyroscopeDriftCompensationEnabled = e.IsChecked;
        }

        private void OnAccelerometerCalibrationSwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.Current.AccelerometerCalibrationEnabled = e.IsChecked;
        }

        private void OnGyroscopeCalibrationSwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.Current.GyroscopeCalibrationEnabled = e.IsChecked;
        }
        #endregion
    }
}  
