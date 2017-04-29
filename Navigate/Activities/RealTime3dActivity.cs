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
    public class RealTime3dActivity : Activity
    {
        #region Views and related fields
        private PaintingView paintingView;
        private Button play3dRTButton;
        private bool pressed = false;

        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.RealTime3dMenu);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            paintingView = FindViewById<PaintingView>(Resource.Id.LogPaintingView);
            play3dRTButton = FindViewById<Button>(Resource.Id.Play3DRealtime);

            play3dRTButton.Click += OnPlay3dRTButtonClicked;

            paintingView.DrawTrajectory = Storage.TrajectoryTracingEnabled;
            paintingView.RealTimeMode = true;
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
        void OnPlay3dRTButtonClicked(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (!pressed)
                {
                    play3dRTButton.Text = "Stop";
                    pressed = true;
                    play3dRTButton.Enabled = false;
                    paintingView.IsPlaying = true;
                }
                else
                {
                    play3dRTButton.Text = "Start";
                    pressed = false;
                    play3dRTButton.Enabled = true;
                    paintingView.IsPlaying = false;
                }
            });
        }
        
        #endregion
    }
}