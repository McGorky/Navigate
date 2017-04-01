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
    [Activity(
#if __ANDROID_11__
        HardwareAccelerated = false,
#endif
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,// LaunchMode = LaunchMode.SingleTask,
        Label = "Log Player",
        Theme = "@style/DarkAndGray")]
    public class LogPlayerActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LogPlayer);

            Spinner dropdown = FindViewById<Spinner>(Resource.Id.spinner1);

            dropdown.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemsSelected);
            var testData = new List<string>() { Storage.CurrentFile };
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, testData);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            dropdown.Adapter = adapter;

            // Load the view
            FindViewById(Resource.Id.paintingview);
        }

        protected override void OnPause()
        {
            base.OnPause();
            var view = FindViewById<PaintingView>(Resource.Id.paintingview);
            view.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            var view = FindViewById<PaintingView>(Resource.Id.paintingview);
            view.Resume();
        }

        private void spinner_ItemsSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner dropdown = (Spinner)sender;
            string toast = string.Format("{0}", dropdown.GetItemAtPosition(e.Position));
            Toast.MakeText(this, toast, ToastLength.Long).Show();

        }
        /*protected void OnBackButtonPressed()
        {
                Finish();
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }
        */
    }
}