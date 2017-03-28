using System.Timers;

using Android.App;
using Android.OS;
using Android.Widget;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "Log Player",
        Theme = "@style/DarkAndGray")]
    public class LogPlayerActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LogPl);

            Spinner dropdown = FindViewById<Spinner>(Resource.Id.spinner1);

            dropdown.ItemSelected += new System.EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemsSelected);
            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.planets_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            dropdown.Adapter = adapter;
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