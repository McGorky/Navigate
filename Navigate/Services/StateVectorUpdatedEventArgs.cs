using System;

namespace Mirea.Snar2017.Navigate.Services
{
    public class StateVectorUpdatedEventArgs : EventArgs
    {
        public StateVector Value { get; }

        public StateVectorUpdatedEventArgs(StateVector value)
        {
            Value = value;
        }
    }
}