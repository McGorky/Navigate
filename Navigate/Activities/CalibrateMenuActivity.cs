using System.Timers;

using Android.App;
using Android.OS;
using Android.Widget;
using System;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "CalibrateMenu",
        Theme = "@style/DarkAndGray")]
    public class CalibrateMenuActivity : Activity
    {
        private Timer timer = new Timer();

        Button frontButton, backButton, topButton, bottomButton, leftButton, rightButton;
        int step;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.CalibrateMenu);
            int count = 0;
            TextView calibrateTextView = FindViewById<TextView>(Resource.Id.CalibrateTextView);

            frontButton = FindViewById<Button>(Resource.Id.CalibrateFrontButton);
            backButton = FindViewById<Button>(Resource.Id.CalibrateBackButton);
            topButton = FindViewById<Button>(Resource.Id.CalibrateTopButton);
            bottomButton = FindViewById<Button>(Resource.Id.CalibrateBottomButton);
            leftButton = FindViewById<Button>(Resource.Id.CalibrateLeftButton);
            rightButton = FindViewById<Button>(Resource.Id.CalibrateRightButton);

            frontButton.Click += frontButtonClicked;
            backButton.Click += backButtonClicked;
            topButton.Click += topButtonClicked;
            bottomButton.Click += bottomButtonClicked;
            leftButton.Click += leftButtonClicked;
            rightButton.Click += rightButtonClicked;

            timer.Elapsed += (o, e) =>
            {
                RunOnUiThread(() => calibrateTextView.Text = $"{++count} seconds elapsed");
            };
            timer.Interval = 1000;
            timer.Enabled = true;

            void frontButtonClicked(object sender, EventArgs e)
            {
                ProgressDialog progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetMessage("Loading...");
                progressBar.SetCancelable(false);
                progressBar.Progress = 0;
                progressBar.Max = 0;
                progressBar.Show();
                step = 0;
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    while (step < 100)
                    {
                        step += 1;
                        progressBar.Progress = step;
                        System.Threading.Thread.Sleep(100);
                    }
                    RunOnUiThread(() => { progressBar.Dismiss(); });
                })).Start();
            }

            void backButtonClicked(object sender, EventArgs e)
            {
                ProgressDialog progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetMessage("Loading...");
                progressBar.SetCancelable(false);
                progressBar.Progress = 0;
                progressBar.Max = 0;
                progressBar.Show();
                step = 0;
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    while (step < 100)
                    {
                        step += 1;
                        progressBar.Progress = step;
                        System.Threading.Thread.Sleep(100);
                    }
                    RunOnUiThread(() => { progressBar.Dismiss(); });
                })).Start();
            }

            void topButtonClicked(object sender, EventArgs e)
            {
                ProgressDialog progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetMessage("Loading...");
                progressBar.SetCancelable(false);
                progressBar.Progress = 0;
                progressBar.Max = 0;
                progressBar.Show();
                step = 0;
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    while (step < 100)
                    {
                        step += 1;
                        progressBar.Progress = step;
                        System.Threading.Thread.Sleep(100);
                    }
                    RunOnUiThread(() => { progressBar.Dismiss(); });
                })).Start();
            }

            void bottomButtonClicked(object sender, EventArgs e)
            {
                ProgressDialog progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetMessage("Loading...");
                progressBar.SetCancelable(false);
                progressBar.Progress = 0;
                progressBar.Max = 0;
                progressBar.Show();
                step = 0;
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    while (step < 100)
                    {
                        step += 1;
                        progressBar.Progress = step;
                        System.Threading.Thread.Sleep(100);
                    }
                    RunOnUiThread(() => { progressBar.Dismiss(); });
                })).Start();
            }

            void leftButtonClicked(object sender, EventArgs e)
            {
                ProgressDialog progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetMessage("Loading...");
                progressBar.SetCancelable(false);
                progressBar.Progress = 0;
                progressBar.Max = 0;
                progressBar.Show();
                step = 0;
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    while (step < 100)
                    {
                        step += 1;
                        progressBar.Progress = step;
                        System.Threading.Thread.Sleep(100);
                    }
                    RunOnUiThread(() => { progressBar.Dismiss(); });
                })).Start();
            }

            void rightButtonClicked(object sender, EventArgs e)
            {
                ProgressDialog progressBar = new ProgressDialog(this);
                progressBar.SetProgressStyle(ProgressDialogStyle.Horizontal);
                progressBar.SetMessage("Loading...");
                progressBar.SetCancelable(false);
                progressBar.Progress = 0;
                progressBar.Max = 0;
                progressBar.Show();
                step = 0;
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    while (step < 100)
                    {
                        step += 1;
                        progressBar.Progress = step;
                        System.Threading.Thread.Sleep(100);
                    }
                    RunOnUiThread(() => { progressBar.Dismiss(); });
                })).Start();
            }



             
        }
        
        
    }
}