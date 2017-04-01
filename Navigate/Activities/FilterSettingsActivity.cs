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
    public class FilterSettingsActivity : Activity
    {
        SeekBar seekbar1, seekbar2, seekbar3;
        EditText madgewickEditText1, madgewickEditText2, exponentialEditText;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FilterSettings);

            seekbar1 = FindViewById<SeekBar>(Resource.Id.seekBar1);
            seekbar2 = FindViewById<SeekBar>(Resource.Id.seekBar2);
            seekbar3 = FindViewById<SeekBar>(Resource.Id.seekBar3);
            madgewickEditText1 = FindViewById<EditText>(Resource.Id.MadgwickEditText1);
            madgewickEditText2 = FindViewById<EditText>(Resource.Id.MadgwickEditText2);
            exponentialEditText = FindViewById<EditText>(Resource.Id.ExponentialEditText);


            RunOnUiThread(() =>
            {
                madgewickEditText1.Text = string.Format("{0:F3}", Storage.Beta);
                madgewickEditText2.Text = string.Format("{0:F3}", Storage.Zeta);
                exponentialEditText.Text = string.Format("{0:F3}", Storage.Gamma);
            });

            madgewickEditText1.RequestFocus();
            Android.Views.InputMethods.InputMethodManager inputManager = (Android.Views.InputMethods.InputMethodManager)GetSystemService(FilterSettingsActivity.InputMethodService);
            inputManager.ShowSoftInput(madgewickEditText1, Android.Views.InputMethods.ShowFlags.Implicit);
            inputManager.ToggleSoftInput(Android.Views.InputMethods.ShowFlags.Forced, Android.Views.InputMethods.HideSoftInputFlags.ImplicitOnly);

            seekbar1.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    if (e.Progress == 0)
                    {
                        Storage.Beta = 0;
                        madgewickEditText1.Text = string.Format("0.000");
                    }
                    else
                    {
                        Storage.Beta = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                        madgewickEditText1.Text = string.Format("{0:F3}", Storage.Beta);
                    }
                }
            };

            seekbar2.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    if (e.Progress == 0)
                    {
                        Storage.Zeta = 0;
                        madgewickEditText2.Text = string.Format("0.000");
                    }
                    else
                    {
                        Storage.Zeta = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                        madgewickEditText2.Text = string.Format("{0:F3}", Storage.Zeta);
                    }
                }
            };

            seekbar3.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    if (e.Progress == 0)
                    {
                        Storage.Gamma = 0;
                        exponentialEditText.Text = string.Format("0.000");
                    }
                    else
                    {
                        Storage.Gamma = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                        exponentialEditText.Text = string.Format("{0:F3}", Storage.Gamma);
                    }
                }
            };

            madgewickEditText1.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                //seekbar1.Progress = Convert.ToInt32(madgewickEditText1.Text);
            };
        }
    }
}  
