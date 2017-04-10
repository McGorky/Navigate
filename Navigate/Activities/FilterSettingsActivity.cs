using System;
using System.Timers;

using Xamarin.Forms;

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
        Theme = "@style/DarkRedAndPink")]
    public class FilterSettingsActivity : Activity
    {
        #region Views and related fields
        SeekBar betaSeekbar,
            zetaSeekbar,
            gammaSeekbar;
        EditText betaEditText,
            zetaEditText,
            gammaEditText;
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.FilterSettings);

            // TODO: привести в пор€док имена в axml
            betaSeekbar = FindViewById<SeekBar>(Resource.Id.BetaSeekBar);
            zetaSeekbar = FindViewById<SeekBar>(Resource.Id.ZetaSeekBar);
            gammaSeekbar = FindViewById<SeekBar>(Resource.Id.ExponentialSeekBar);
            betaEditText = FindViewById<EditText>(Resource.Id.MadgwickEditText1);
            zetaEditText = FindViewById<EditText>(Resource.Id.MadgwickEditText2);
            gammaEditText = FindViewById<EditText>(Resource.Id.ExponentialEditText);

            RunOnUiThread(() =>
            {
                betaEditText.Text = $"{Storage.Beta:F3}";
                zetaEditText.Text = $"{Storage.Zeta:F3}";
                gammaEditText.Text = $"{Storage.Gamma:F3}";
            });

            // TODO: создать обработчики вместо л€мбд, поместить в Handlers (внизу)
            // TODO: заменить все string.Format на интерпол€цию строк
            // string.Format("{0:F3}", Storage.Beta)
            //           V
            // $"{Storage.Beta:F3}"
            betaSeekbar.ProgressChanged += OnBetaProgressChanged;
            zetaSeekbar.ProgressChanged += OnZetaProgressChanged;
            gammaSeekbar.ProgressChanged += OnGammaProgressChanged;

            

            /* betaEditText.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) =>
            {
                // нужно использовать int.Parse();
                betaSeekbar.Progress = Convert.ToInt32(betaEditText.Text);
            };
            */
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
        void OnBetaProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                if (e.Progress == 0)
                {
                    Storage.Beta = 0;
                    betaEditText.Text = string.Format("0.000");
                }
                else
                {
                    Storage.Beta = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    betaEditText.Text = string.Format("{0:F3}", Storage.Beta);
                }
            }
        }


        void OnZetaProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                if (e.Progress == 0)
                {
                    Storage.Zeta = 0;
                    zetaEditText.Text = string.Format("0.000");
                }
                else
                {
                    Storage.Zeta = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    zetaEditText.Text = string.Format("{0:F3}", Storage.Zeta);
                }
            }
        }


        void OnGammaProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                if (e.Progress == 0)
                {
                    Storage.Gamma = 0;
                    gammaEditText.Text = string.Format("0.000");
                }
                else
                {
                    Storage.Gamma = ((e.Progress * e.Progress) / 200) * (float)Math.Log10(e.Progress) / 15000f;
                    gammaEditText.Text = string.Format("{0:F3}", Storage.Gamma);
                }
            }
        }
        #endregion
    }
}  
