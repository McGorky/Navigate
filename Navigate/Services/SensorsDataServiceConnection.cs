using System;

using Android.Content;
using Android.OS;

namespace Mirea.Snar2017.Navigate.Services
{
    public class SensorsDataServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public event EventHandler<ServiceConnectedEventArgs> ServiceConnected; // = delegate { };

        public SensorsDataServiceBinder Binder { get; protected set; }

        public SensorsDataServiceConnection(SensorsDataServiceBinder binder)
        {
            if (Binder != null)
            {
                Binder = binder;
            }
        }

        // This gets called when a client tries to bind to the Service with an Intent and an 
        // instance of the ServiceConnection. The system will locate a binder associated with the 
        // running Service 
        public void OnServiceConnected(ComponentName name, IBinder binder)
        {
            // cast the binder located by the OS as our local binder subclass
            if (binder is SensorsDataServiceBinder serviceBinder)
            {
                Binder = serviceBinder;
                Binder.IsBound = true;
                ServiceConnected?.Invoke(this, new ServiceConnectedEventArgs() { Binder = binder });
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Binder.IsBound = false;
            //Log.Debug("ServiceConnection", "Service unbound");
        }
    }
}