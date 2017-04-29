using System;

using OpenTK;

namespace Mirea.Snar2017.Navigate
{
    public class StateVector
    {
        /// <summary>
        /// Time in seconds
        /// </summary>
        public float DeltaTime { get; set; }
        public Quaternion Orientation { get; set; }
        public OpenTK.Vector3 Position { get; set; }
        public float[] Velocity { get; set; }
        public float[] Acceleration { get; set; }
    }
}