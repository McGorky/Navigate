using System;
using System.Timers;

//using Xamarin.Forms;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;
using Android.Text;
using System.Collections.Generic;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "Filter Settings",
        Theme = "@style/DarkRedAndPink")]
    public class RecordsMenuActivity : Activity
    {
        #region Views and related fields
        private Button showPlotsButton;
        private Button show3DButton;
        private Spinner logSelectSpinner;
        private List<string> SavedLogs
        {
            // UNDONE: искать в папке логи возвращать список с их именами
            get;
        } = new List<string>() { "A", "B" };
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.RecordsMenu);

            showPlotsButton = FindViewById<Button>(Resource.Id.ShowPlotsButton);
            show3DButton = FindViewById<Button>(Resource.Id.Show3DButton);

            showPlotsButton.Click += OnShowPlotsButtonClicked;
            show3DButton.Click += OnShow3DButtonClicked;

            logSelectSpinner = FindViewById<Spinner>(Resource.Id.LogNameSpinner);

            logSelectSpinner.ItemSelected += OnSpinnerItemSelected; //new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemsSelected);
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, SavedLogs);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            logSelectSpinner.Adapter = adapter;
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
        private void OnSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner dropdown = (Spinner)sender;
            string toast = string.Format("{0}", dropdown.GetItemAtPosition(e.Position));
            Toast.MakeText(this, toast, ToastLength.Short).Show();
        }
        #endregion
    }
}  
