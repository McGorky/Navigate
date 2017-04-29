using System;

using OpenTK;

namespace Mirea.Snar2017.Navigate
{
    public class SensorsReadings
    {
        /// <summary>
        /// Time in seconds
        /// </summary>
        public float Time { get; set; }
        /// <summary>
        /// Acceleration in m/s^2
        /// </summary>
        public Vector3 Acceleration { get; set; }
        /// <summary>
        /// Linear acceleration in m/s^2
        /// </summary>
        public Vector3 LinearAcceleration { get; set; }
        /// <summary>
        /// Angular velocity in rad/s
        /// </summary>
        public Vector3 AngularVelocity { get; set; }
        /// <summary>
        /// Flux in uT
        /// </summary>
        public Vector3 MagneticField { get; set; }
    }
}