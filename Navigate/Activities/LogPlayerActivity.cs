using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Content.PM;

namespace Mirea.Snar2017.Navigate
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,// LaunchMode = LaunchMode.SingleTask,
        Label = "LogPlayer",
        Theme = "@style/DarkRedAndPink")]
    public class LogPlayerActivity : Activity
    {
        #region Views and related fields
        private PaintingView paintingView;
        private NumberPicker speedPicker;
        private Button playStopButton;
        private SeekBar rewindSeekbar;
        private bool pressed = false;

        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LogPlayer);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            paintingView = FindViewById<PaintingView>(Resource.Id.LogPaintingView);
            speedPicker = FindViewById<NumberPicker>(Resource.Id.SpeedPicker);
            playStopButton = FindViewById<Button>(Resource.Id.Play3dButton);
            rewindSeekbar = FindViewById<SeekBar>(Resource.Id.seekBar1);

            rewindSeekbar.Enabled = false;

            speedPicker.MinValue = 0;
            speedPicker.MaxValue = 7;
            speedPicker.WrapSelectorWheel = false;
            var speeds = new string[] { "0.25", "0.5", "0.75", "1", "1.25", "1.5", "1.75", "2" };
            speedPicker.SetDisplayedValues(speeds);

            playStopButton.Click += OnPlayStopButtonClicked;

            paintingView.DrawTrajectory = Storage.TrajectoryTracingEnabled;

            speedPicker.Value = 3;
            speedPicker.ValueChanged += (o, e) =>
            {
                paintingView.SpeedMultiplier = float.Parse(speeds[speedPicker.Value]);
            };

            rewindSeekbar.Max = Storage.NumberOfFrames;
            paintingView.CoordinatesUpdated += () => RunOnUiThread(() =>
            {
                rewindSeekbar.Progress = Storage.CurrentFrame;
                rewindSeekbar.RefreshDrawableState();
            });

            paintingView.Finished += () =>
            {
                OnPlayStopButtonClicked(playStopButton, null);
            };

            rewindSeekbar.ProgressChanged += (o, e) =>
            {
                Storage.CurrentFrame = rewindSeekbar.Progress;
            };
        }

        protected override void OnPause()
        {
            paintingView.Pause();
            base.OnPause();
        }

        protected override void OnResume()
        {
            paintingView.Pause();
            base.OnResume();
        }

        protected override void OnStop()
        {
            paintingView.Stop();
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

        #region Handlers
        void OnPlayStopButtonClicked(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (!pressed)
                {
                    playStopButton.Text = "Stop";
                    pressed = true;
                    //rewindSeekbar.Enabled = false;
                    speedPicker.Enabled = false;
                    paintingView.IsPlaying = true;
                }
                else
                {
                    playStopButton.Text = "Start";
                    pressed = false;
                    //rewindSeekbar.Enabled = true;
                    speedPicker.Enabled = true;
                    paintingView.IsPlaying = false;
                }
            });
        }
        #endregion
    }
}