using System;
using System.Collections.Generic;

namespace Mirea.Snar2017.Navigate
{
    public class Storage
    {
        public static float[] AccelerometerData = new float[3];
        public static float[] GyroscopeData = new float[3];
        public static float[] MagnetometerData = new float[3];

        public static Matrix AccelerometerCalibrationMatrix;

        public static float Beta;
        public static float Zeta;
        public static float Gamma;

        public static TimeSpan Uptime = new TimeSpan();

        public static DateTime StartTime;
    }
}