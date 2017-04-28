using System;

using OpenTK;

namespace Mirea.Snar2017.Navigate
{
    public class StateVector
    {
        public DateTime Time { get; set; }
        public OpenTK.Quaternion Orientation { get; set; }
        public OpenTK.Vector3 Position { get; set; }
    }
}