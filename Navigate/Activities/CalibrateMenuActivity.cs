using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "CalibrateMenu",
            Theme = "@style/DarkAndGray")]
    public class CalibrateMenuActivity : Activity
    {
        private Timer timer = new Timer();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.CalibrateMenu);
            int count = 0;
            TextView calibrateView = FindViewById<TextView>(Resource.Id.CalibrateText);
            Button button = FindViewById<Button>(Resource.Id.CalibrateExit);

            timer.Elapsed += (o, e) =>
            {
                RunOnUiThread(() => calibrateView.Text = $"{++count} seconds elapsed");
            };
            timer.Interval = 1000;
            timer.Enabled = true;
            button.Click += (o, e) =>
            {
                Finish();
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
            };
        }
    }
}