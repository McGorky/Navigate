using System.Timers;

using Android.App;
using Android.OS;
using Android.Widget;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "Filter Settings",
        Theme = "@style/DarkAndGray")]
    public class FilSetsActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FilterSets);
            
                
        }

        /*protected void OnBackButtonPressed()
        {
                Finish();
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }
        */
        
        
    }
}