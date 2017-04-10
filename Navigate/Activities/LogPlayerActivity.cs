using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LogPlayer);

            paintingView = FindViewById<PaintingView>(Resource.Id.LogPaintingView);
       
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

        #endregion
    }
}