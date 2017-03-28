using System.Timers;
using Xamarin.Forms;

using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;
using Android.Text;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "Filter Settings",
        Theme = "@style/DarkAndGray")]
    public class FilSetsActivity : Activity

    {
        SeekBar sbar1, sbar2, sbar3;
        EditText madgtxt1, madgtxt2, exptxt;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FilterSets);

            sbar1 = FindViewById<SeekBar>(Resource.Id.seekBar1);
            sbar2 = FindViewById<SeekBar>(Resource.Id.seekBar2);
            sbar3 = FindViewById<SeekBar>(Resource.Id.seekBar3);
            madgtxt1 = FindViewById<EditText>(Resource.Id.MadgwickTxt);
            madgtxt2 = FindViewById<EditText>(Resource.Id.MadgwickTxt2);
            exptxt = FindViewById<EditText>(Resource.Id.ExponentialText);

            sbar1.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
              {
                  if (e.FromUser)
                  {
                      madgtxt1.Text = string.Format("{0:F2}",e.Progress/100f);
                  }
              };

            sbar2.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    madgtxt2.Text = string.Format("{0:F2}", e.Progress / 100f);
                }
            };

            sbar3.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    exptxt.Text = string.Format("{0:F2}", e.Progress / 100f);
                }
            };

            madgtxt1.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
              {
                 // SetProgress(Convert.ToInt32(etxt.Text));
                 // sbar.SetProgress(1);
                  //sbar.Progress = Convert.ToInt32(etxt.Text);
              };
          
            
        }
        


    }


}  
