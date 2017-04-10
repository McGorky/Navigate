using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Hardware;
using Android.Widget;
using Android.Runtime;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "LogMenu",
        Theme = "@style/DarkRedAndPink")]
    public class LogMenuActivity : Activity
    {
        #region Views and related fields
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LogMenu);
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

        #region Handlers      

        #endregion
    }
}