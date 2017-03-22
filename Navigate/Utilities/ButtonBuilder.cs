using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.Animations;

namespace Mirea.Snar2017.Navigate
{
    public static class ButtonBuilder
    {
        public static Button Create(Activity currentActivity,
            int buttonId,
            int pressAnimationId = Resource.Animation.Expand,
            int downAnimationId = Resource.Animation.Shrink)
        {
            Button output = currentActivity.FindViewById<Button>(buttonId);
            output.Touch += (o, e) =>
            {
                Animation animation;
                switch (e.Event.Action)
                {
                    case MotionEventActions.Down:
                    {
                        animation = AnimationUtils.LoadAnimation(currentActivity.ApplicationContext, downAnimationId);
                        output.StartAnimation(animation);
                        break;
                    }
                    case MotionEventActions.Up:
                    {
                        animation = AnimationUtils.LoadAnimation(currentActivity.ApplicationContext, pressAnimationId);
                        output.StartAnimation(animation);
                        break;
                    }
                }
            };
            return output;
        }
    }
}