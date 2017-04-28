using Android.OS;

namespace Mirea.Snar2017.Navigate.Services
{
    public class SensorsDataServiceBinder : Binder
    {
        public SensorsDataService Service { get; protected set; }

        public bool IsBound { get; set; }

        public SensorsDataServiceBinder(SensorsDataService service)
        {
            Service = service;
        }
    }
}