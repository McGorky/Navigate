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

            betaEditText = FindViewById<EditText>(Resource.Id.MadgwickBetaEditText);
            zetaEditText = FindViewById<EditText>(Resource.Id.MadgwickZetaEditText);
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

                betaEditText.Text = $"{Storage.Beta:F3}";
                zetaEditText.Text = $"{Storage.Zeta:F3}";
                gammaEditText.Text = $"{Storage.Gamma:F3}";

                betaSeekbar.Max = 1000;
                betaSeekbar.Progress = (int)(Storage.Beta * 1000);
                betaSeekbar.RefreshDrawableState();

                zetaSeekbar.Max = 1000;
                zetaSeekbar.Progress = (int)(Storage.Zeta * 1000);
                zetaSeekbar.RefreshDrawableState();

                gammaSeekbar.Max = 1000;
                gammaSeekbar.Progress = (int)(Storage.Gamma * 1000);
                gammaSeekbar.RefreshDrawableState();

                magnetometerSwitch.Checked = Storage.MagnetometerEnabled;
                gyroscopeDriftSwitch.Checked = Storage.GyroscopeDriftCompensationEnabled;
                accelerometerCalibrationSwitch.Checked = Storage.AccelerometerCalibrationEnabled;
                gyroscopeCalibrationSwitch.Checked = Storage.GyroscopeCalibrationEnabled;
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
                    Storage.Beta = 0;
                    betaEditText.Text = $"{0.0f:F3}"; ;
                }
                else
                {
                    //Storage.Beta = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    Storage.Beta = e.Progress / 1000.0f;
                    betaEditText.Text = $"{Storage.Beta:F3}";
                }
            }
        }

        void OnZetaProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                if (e.Progress == 0)
                {
                    Storage.Zeta = 0;
                    zetaEditText.Text = string.Format("0.000");
                }
                else
                {
                    //Storage.Zeta = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    Storage.Zeta = e.Progress / 1000.0f;
                    zetaEditText.Text = string.Format("{0:F3}", Storage.Zeta);
                }
            }
        }

        void OnGammaProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                if (e.Progress == 0)
                {
                    Storage.Gamma = 0;
                    gammaEditText.Text = string.Format("0.000");
                }
                else
                {
                    //Storage.Gamma = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    Storage.Gamma = e.Progress / 1000.0f;
                    gammaEditText.Text = string.Format("{0:F3}", Storage.Gamma);
                }
            }
        }

        private void OnMagnetometerSwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.MagnetometerEnabled = e.IsChecked;
        }

        private void OnGyroscopeDriftSwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.GyroscopeDriftCompensationEnabled = e.IsChecked;
        }

        private void OnAccelerometerCalibrationSwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.AccelerometerCalibrationEnabled = e.IsChecked;
        }

        private void OnGyroscopeCalibrationSwitchCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Storage.GyroscopeCalibrationEnabled = e.IsChecked;
        }
        #endregion
    }
}  
