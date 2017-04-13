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

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Mirea.Snar2017.Navigate
{
    [Activity(Label = "LogMenu",
        Theme = "@style/DarkRedAndPink")]
    public class LogMenuActivity : Activity
    {
        #region Views and related fields
        PlotView psiPlot,
            thetaPlot,
            phiPlot;
        #endregion

        #region Activity methods
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.LogMenu);

            //var offsetPlot = FindViewById<PlotView>(Resource.Id.PlotViewS);
            psiPlot = FindViewById<PlotView>(Resource.Id.PlotViewPsi);
            thetaPlot = FindViewById<PlotView>(Resource.Id.PlotViewTheta);
            phiPlot = FindViewById<PlotView>(Resource.Id.PlotViewFi);

            psiPlot.Model = CreatePlotModel("Time", "s", "Psi", "Deg", Storage.Psi);
            thetaPlot.Model = CreatePlotModel("Time", "s", "Theta", "Deg", Storage.Theta);
            phiPlot.Model = CreatePlotModel("Time", "s", "Phi", "Deg", Storage.Phi);

            psiPlot.InvalidatePlot();
            thetaPlot.InvalidatePlot();
            phiPlot.InvalidatePlot();

            //var axlPlot = FindViewById<PlotView>(Resource.Id.PlotViewAccelerometer);
            //var gyroplot = FindViewById<PlotView>(Resource.Id.PlotViewGyroscope);
            //var magnPlot = FindViewById<PlotView>(Resource.Id.PlotViewMagnetometer);
        }

        private PlotModel CreatePlotModel(string xName, string xUnits, string yName, string yUnits, LineSeries series)
        {
            var plotModel = new PlotModel();
            double fontSize = 7;

            var timeAxis = new LinearAxis { Position = AxisPosition.Bottom, FontSize = fontSize, Title = $"{xName}, {xUnits}", MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };
            var valueAxis = new LinearAxis { Position = AxisPosition.Left, FontSize = fontSize, Title = $"{yName}, {yUnits}", MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };

            plotModel.Axes.Add(timeAxis);
            plotModel.Axes.Add(valueAxis);

            plotModel.Series.Add(series);

            return plotModel;
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

            psiPlot.Model.Series.Remove(Storage.Psi);
            thetaPlot.Model.Series.Remove(Storage.Theta);
            phiPlot.Model.Series.Remove(Storage.Phi);
            OverridePendingTransition(Resource.Animation.ExpandIn, Resource.Animation.ShrinkOut);
        }
        #endregion

        #region Handlers      

        #endregion
    }
}