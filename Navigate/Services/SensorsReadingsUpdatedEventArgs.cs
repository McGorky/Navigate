using System;

using OpenTK;

namespace Mirea.Snar2017.Navigate.Services
{
    public class SensorsReadingsUpdatedEventArgs : EventArgs
    {
        public SensorsReadings Value { get; }

        public SensorsReadingsUpdatedEventArgs(SensorsReadings value)
        {
            Value = value;
        }
    }
}