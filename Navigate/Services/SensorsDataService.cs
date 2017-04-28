using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Media;

namespace Mirea.Snar2017.Navigate.Services
{
    [Service]
    public class SensorsDataService : Service, ISensorEventListener
    {
        public event EventHandler<SensorsReadingsUpdatedEventArgs> SensorsReadingsUpdated;
        public event EventHandler<StateVectorUpdatedEventArgs> StateVectorUpdated;

        public RawData RawData { get; set; } = new RawData();
        public FilteredData FilteredData { get; set; } = new FilteredData();

        public bool CalculatingOrientation { get; set; } = false;

        private bool accelerometerDataGathered = false;
        private bool linearAccelerationDataGathered = false;
        private bool gyroscopeDataGathered = false;
        private bool magneticFieldDataGathered = false;

        private OpenTK.Vector3 acceleration = new OpenTK.Vector3();
        private OpenTK.Vector3 linearAcceleration = new OpenTK.Vector3();
        private OpenTK.Vector3 angularVelocity = new OpenTK.Vector3();
        private OpenTK.Vector3 magneticField = new OpenTK.Vector3();

        private bool first = true;

        IBinder binder;

        private SensorManager sensorManager;

        #region Service methods
        public override void OnCreate()
        {
            base.OnCreate();
            StartForeground(Storage.ForegroundServiceId.SensorsData, new Notification());

            FilteredData.RawDataLog = RawData;

            sensorManager = (SensorManager)GetSystemService(Context.SensorService);

            EnableSensors();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            binder = new SensorsDataServiceBinder(this);
            return binder;
        }

        public override void OnDestroy()
        {
            DisableSensors();

            MediaScannerConnection.ScanFile(this, new string[] { Storage.RawFolderName }, null, null);
            StopForeground(true);

            // REMARK I:
            //Serialize: rawData, filteredData
            //rawData.ToCsv()
            //filteredData.ToCsv()

            base.OnDestroy();
        }
        #endregion

        #region ISensorEventListener
        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (first)
            {
                first = false;
                Storage.StartTimestamp = e.Timestamp;
            }
            var values = new OpenTK.Vector3()
            {
                X = e.Values[0],
                Y = e.Values[1],
                Z = e.Values[2]
            };

            switch (e.Sensor.Type)
            {
                case SensorType.Accelerometer:
                {
                    acceleration = values;
                    accelerometerDataGathered = true;
                    break;
                }
                case SensorType.LinearAcceleration:
                {
                    linearAcceleration = values;
                    linearAccelerationDataGathered = true;
                    break;
                }
                case SensorType.Gyroscope: // SensorType.GyroscopeUncalibrated: - раскомментить RegisterListener
                {
                    angularVelocity = values;
                    gyroscopeDataGathered = true;
                    break;
                }
                case SensorType.MagneticField:
                {
                    magneticField = values;
                    magneticFieldDataGathered = true;
                    break;
                }
            }

            if (accelerometerDataGathered && linearAccelerationDataGathered && gyroscopeDataGathered && magneticFieldDataGathered)
            {
                var sensorsReadings = new SensorsReadings
                {
                    Time = (e.Timestamp - Storage.StartTimestamp) * 1e-9,
                    Acceleration = acceleration,
                    LinearAcceleration = linearAcceleration,
                    AngularVelocity = angularVelocity,
                    MagneticField = magneticField
                };
                SensorsReadingsUpdated(this, new SensorsReadingsUpdatedEventArgs(sensorsReadings));

                accelerometerDataGathered = false;
                linearAccelerationDataGathered = false;
                gyroscopeDataGathered = false;
                magneticFieldDataGathered = false;

                if (CalculatingOrientation)
                {
                    var thread = new System.Threading.Thread(() =>
                    {
                        // TODO:
                        // 1.
                        // Apply calibration to accelerometer and gyro : a, g -> aC, gC
                        // Get orientation quaternion using Madgvick filter : aC, gC, m -> q
                        // Get linear acceleration from accelerometer using high pass filter: aC -> aL
                        // Rotate linear acceleration with orientation quaternion : aL, q -> aLG
                        // Integrate global linear acceleration to get global offset : aLG -> s
                        // Raise StateVectorUpdated event
                        // 2.
                        // Add data to log
                        // 3.

                        RawData.Add(sensorsReadings);

                        var filtered = new StateVector();
                        FilteredData.Add(filtered);
                        StateVectorUpdated(this, new StateVectorUpdatedEventArgs(filtered));
                    });
                }
            }
        }
        #endregion

        private void EnableSensors()
        {
            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                                            SensorDelay.Game, 30);
            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.LinearAcceleration),
                                            SensorDelay.Game, 30);
            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.Gyroscope),
                                            SensorDelay.Game, 30);
            //sensorManager.RegisterListener(this,
            //                                sensorManager.GetDefaultSensor(SensorType.GyroscopeUncalibrated),
            //                                SensorDelay.Game, 30);
            sensorManager.RegisterListener(this,
                                            sensorManager.GetDefaultSensor(SensorType.MagneticField),
                                            SensorDelay.Game, 30);
        }

        private void DisableSensors()
        {
            sensorManager.UnregisterListener(this);
        }

        public void Restart()
        {
            DisableSensors();

            accelerometerDataGathered = false;
            linearAccelerationDataGathered = false;
            gyroscopeDataGathered = false;
            magneticFieldDataGathered = false;

            acceleration = new OpenTK.Vector3();
            linearAcceleration = new OpenTK.Vector3();
            angularVelocity = new OpenTK.Vector3();
            magneticField = new OpenTK.Vector3();

            EnableSensors();
        }

        private void AddToLog()
        {
            throw new NotImplementedException();
        }
    }
}