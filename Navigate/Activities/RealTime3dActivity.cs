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
        private TextView stateVectorTextView,
            drawTrajectoryTextView,
            freeCameraTextView;
        private Switch drawTrajectorySwitch,
            freeCameraSwitch;

        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.RealTime3dMenu);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            paintingView = FindViewById<PaintingView>(Resource.Id.LogPaintingView);
            stateVectorTextView = FindViewById<TextView>(Resource.Id.StateVectorTextViewRealTime);
            drawTrajectoryTextView = FindViewById<TextView>(Resource.Id.DrawTrajectoryTextViewRealTime);
            freeCameraTextView = FindViewById<TextView>(Resource.Id.FreeCameraTextViewRealTime);
            drawTrajectorySwitch = FindViewById<Switch>(Resource.Id.DrawTrajectorySwitchRealTime);
            freeCameraSwitch = FindViewById<Switch>(Resource.Id.FreeCameraSwitchRealTime);

            drawTrajectorySwitch.CheckedChange += (o, e) => paintingView.DrawTrajectory = e.IsChecked;
            freeCameraSwitch.CheckedChange += (o, e) => paintingView.FreeCamera = e.IsChecked;

            paintingView.DrawTrajectory = Storage.TrajectoryTracingEnabled;
            Storage.Current.SensorsDataService.CalculatingOrientation = true;
            paintingView.RealTimeMode = true;
            paintingView.Start();
            Storage.Current.SensorsDataService.CalculatingOrientation = true;
            //paintingView.FreeCamera = false;
            Storage.Current.SensorsDataService.StateVectorUpdated += (o, e) =>
            {
                paintingView.UpdateCoordinates(e.Value);
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
            Storage.Current.SensorsDataService.CalculatingOrientation = false;
            Storage.Current.SensorsDataService.CalculatingOrientation = false;
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

        
        #endregion
    }
}