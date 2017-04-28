using System;

using Android.OS;

namespace Mirea.Snar2017.Navigate.Services
{
    public class ServiceConnectedEventArgs : EventArgs
    {
        public IBinder Binder { get; set; }
    }
}