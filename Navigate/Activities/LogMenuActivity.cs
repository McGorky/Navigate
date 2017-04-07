using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Hardware;
using Android.Widget;
using Android.Runtime;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "LogMenu",
        Theme = "@style/DarkRedAndPink")]
    public class LogMenuActivity : Activity
    {
        // REMARK KK: зачем? и camelCase
        System.Diagnostics.Stopwatch logstopwatch = new System.Diagnostics.Stopwatch();

        // REMARK KK: привести в пор€док имена (пкм - переименовать)
        #region Views and related fields
        private Button playStopButton;
        private TextView logStopwatchText;
        private EditText input;
        bool pressed = false;
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LogMenu);

            logStopwatchText = FindViewById<TextView>(Resource.Id.LogTimer);
            playStopButton = FindViewById<Button>(Resource.Id.PlayStopButton);
            playStopButton.Click += OnPlayStopClicked;
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
        // REMARK KK: интерпол€ци€ строк
        void OnPlayStopClicked(object sender, EventArgs e)
        {
            if (!pressed)
            {
                logstopwatch.Start();
                logStopwatchText.Text = string.Format("{0}:{1}", logstopwatch.Elapsed.Seconds, logstopwatch.Elapsed.Milliseconds);
                playStopButton.Text = "Stop";
                pressed = true;
            }
            else
            {
                logstopwatch.Stop();
                playStopButton.Text = "Play";
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Name");
                input = new EditText(this);
                builder.SetView(input);
                input.Text = "f.txt";

                builder.SetPositiveButton("OK", OkAction);
                builder.SetNegativeButton("Cancel", CancelAction);
                builder.Show();

                pressed = false;
            }
        }

        // REMARK KK: привести в пор€док имена - <blabla>OkAction
        private void OkAction(object sender, DialogClickEventArgs e)
        {
        }

        private void CancelAction(object sender, DialogClickEventArgs e)
        {
        }
        #endregion
    }
}