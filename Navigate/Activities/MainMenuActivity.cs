using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "@string/ApplicationName",
            MainLauncher = true,
            Icon = "@drawable/ApplicationIcon",
            Theme = "@style/DarkAndGray"
            )]
    public class MainMenuActivityActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainMenu);

            Button calibrateButton = FindViewById<Button>(Resource.Id.CalibrateButton);
            Button filterButton = FindViewById<Button>(Resource.Id.FilterSettingsButton);
            Button logButton = ButtonBuilder.Create(this, Resource.Id.LogButton);
            logButton.Text = "sample text";

            bool p = false, p2 = false;

            calibrateButton.Touch += (o, e) =>
            {
                Animation animation;
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                    {
                        animation = AnimationUtils.LoadAnimation(ApplicationContext, Resource.Animation.Shrink);
                        calibrateButton.StartAnimation(animation);
                        break;
                    }
                    case MotionEventActions.Up:
                    {
                        animation = AnimationUtils.LoadAnimation(ApplicationContext, Resource.Animation.Expand);
                        calibrateButton.StartAnimation(animation);

                        var intent = new Intent(this, typeof(CalibrateMenuActivity));
                        StartActivity(intent);
                        OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
                        break;
                    }
                }
            };
            calibrateButton.Click += (o, e) =>
            {
                calibrateButton.Text = p ? "5" : "6";
                p = !p;
            };

            logButton.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                    {
                        logButton.Text = p2 ? "1" : "2";
                        p2 = !p2;
                        break;
                    }
                    case MotionEventActions.ButtonRelease:
                    {
                        logButton.Text = p ? "3" : "4";
                        p = !p;
                        break;
                    }
                }
            };
            logButton.Click += (o, e) =>
            {
                logButton.Text = p2 ? "5" : "6";
                p2 = !p2;
            };
        }
    }
}