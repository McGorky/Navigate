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
        Theme = "@style/DarkAndGray")]
    public class LogMenuActivity : Activity
    {
        System.Diagnostics.Stopwatch logstopwatch = new System.Diagnostics.Stopwatch();

        Button playStop;
        EditText logStopwatchText;
        bool pressed = false;
        int count;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LogMenu);

            logStopwatchText = FindViewById<EditText>(Resource.Id.LogTimer);
            playStop = FindViewById<Button>(Resource.Id.PlayStopButton);
            playStop.Click += OnPlayStopClicked;

        }

        private EditText inp;
        void OnPlayStopClicked(object sender, EventArgs e)
        {
            if (!pressed)
            {
                logstopwatch.Start();
                logStopwatchText.Text = string.Format("{0}:{1}", logstopwatch.Elapsed.Seconds, logstopwatch.Elapsed.Milliseconds);
                playStop.Text = "Stop";
                pressed = true;
            }
            else
            {
                logstopwatch.Stop();
                
                playStop.Text = "Play";
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Name");
                inp = new EditText(this);
                builder.SetView(inp);
                inp.Text = "f.txt";

                builder.SetPositiveButton("OK", OkAction);
                builder.SetNegativeButton("Cancel", CancelAction);
                builder.Show();

                pressed = false;
            }
        }


        private void OkAction(object sender, DialogClickEventArgs e)
        {
        }

        private void CancelAction(object sender, DialogClickEventArgs e)
        {
        }

    }
}