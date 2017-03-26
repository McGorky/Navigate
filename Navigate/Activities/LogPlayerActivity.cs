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
            
                
        }

        /*protected void OnBackButtonPressed()
        {
                Finish();
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }
        */
        
        
    }
}