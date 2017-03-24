using System.Timers;

using Android.App;
using Android.OS;
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
            TextView calibrateTextView = FindViewById<TextView>(Resource.Id.CalibrateTextView);
            Button exitButton = FindViewById<Button>(Resource.Id.CalibrateExitButton);
            Button calibrateXButton = ButtonBuilder.Create(this, Resource.Id.CalibrateXButton);
            Button calibrateYButton = ButtonBuilder.Create(this, Resource.Id.CalibrateYButton);
            Button calibrateZButton = ButtonBuilder.Create(this, Resource.Id.CalibrateZButton);


            timer.Elapsed += (o, e) =>
            {
                RunOnUiThread(() => calibrateTextView.Text = $"{++count} seconds elapsed");
            };
            timer.Interval = 1000;
            timer.Enabled = true;
            exitButton.Click += (o, e) =>
            {
                Finish();
                OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
            };
        }
    }
}