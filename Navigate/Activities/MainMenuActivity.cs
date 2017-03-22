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
    public class MainMenuActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainMenu);

            Button calibrateButton = ButtonBuilder.Create(this, Resource.Id.CalibrateButton);
            Button filterButton = ButtonBuilder.Create(this, Resource.Id.FilterSettingsButton);
            Button logButton = ButtonBuilder.Create(this, Resource.Id.LogButton);
            Button logPlayerButton = ButtonBuilder.Create(this, Resource.Id.ShowLogPlayerButton);
            Button clearButton = ButtonBuilder.Create(this, Resource.Id.ClearLogButton);

            calibrateButton.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                    {
                        break;
                    }
                    case MotionEventActions.Up:
                    {
                        var intent = new Intent(this, typeof(CalibrateMenuActivity));
                        StartActivity(intent);
                        OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
                        break;
                    }
                }
            };
        }
    }
}