using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

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

        private bool calculatingOrientation = false;
        public bool CalculatingOrientation
        {
            get => calculatingOrientation;
            set
            {
                RawData.AccelerometerCalibrationMatrix = Storage.Current.AccelerometerCalibrationMatrix;
                RawData.GyroscopeCalibrationVector = Storage.Current.GyroscopeCalibrationVector;
                calculatingOrientation = value;
            }
        }

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
                    Time = (e.Timestamp - Storage.StartTimestamp) * 1e-9f,
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
                    new System.Threading.Thread(() =>
                    {
                        lock (Storage.DataAccessSync)
                        {
                            RawData.Add(sensorsReadings);

                            var acc = sensorsReadings.Acceleration;
                            var a = new Quaternion(0, acc.X, acc.Y, acc.Z);
                            a = Filter.Calibrate(RawData.AccelerometerCalibrationMatrix, a);
                            var gyro = sensorsReadings.AngularVelocity - RawData.GyroscopeCalibrationVector;
                            var g = new Quaternion(0, gyro.X, gyro.Y, gyro.Z);
                            var flux = sensorsReadings.MagneticField;
                            var m = new Quaternion(0, flux.X, flux.Y, flux.Z) * 1e-6f;

                            var q = FilteredData.Data.Count != 0 ? FilteredData.Data[FilteredData.Data.Count - 1].Orientation : new Quaternion(1, 0, 0, 0);
                            float dt = RawData.Data.Count >= 2 ? RawData.Data[RawData.Data.Count - 1].Time - RawData.Data[RawData.Data.Count - 2].Time : 0;
                            q = Filter.Madgvic(q, g, a, m, Storage.Current.Beta, Storage.Current.Zeta, dt);

                            var localRotation = Quaternion.CalculateDifference(new Quaternion(1, 0, 0, 0), q).Normalized();
                            var alin = sensorsReadings.LinearAcceleration;
                            var aLinear = (0, alin.X, alin.Y, alin.Z);
                            var aGlobal = localRotation * aLinear * localRotation.Conjugated();

                            var previousAcc = FilteredData.Data.Count != 0 ? FilteredData.Data[FilteredData.Data.Count - 1].Acceleration : new float[3];
                            var previousVel = FilteredData.Data.Count != 0 ? FilteredData.Data[FilteredData.Data.Count - 1].Velocity : new float[3];
                            var previousOffset = FilteredData.Data.Count != 0 ? FilteredData.Data[FilteredData.Data.Count - 1].Position : new OpenTK.Vector3(0, 0, 0);
                            var accelerationCurrent = Filter.Exponential(new float[] { alin.X, alin.Y, alin.Z }, new float[] { aGlobal.X, aGlobal.Y, aGlobal.Z }, Storage.Current.Gamma);
                            var velocityCurrent = Filter.Integrate(previousAcc, accelerationCurrent, previousVel, dt);
                            var offset = Filter.Integrate(previousVel, velocityCurrent, new float[] { previousOffset.X, previousOffset.Y, previousOffset.Z }, dt);
                            var position = new OpenTK.Vector3(offset[0], offset[1], offset[2]);

                            var filtered = new StateVector { Orientation = q, DeltaTime = dt, Position = position, Acceleration = accelerationCurrent, Velocity = velocityCurrent };
                            FilteredData.Add(filtered);
                            StateVectorUpdated(this, new StateVectorUpdatedEventArgs(filtered));
                        }
                    }).Start();
                }
            }
        }
        #endregion

        public void SaveData(string fileName)
        {
            using (FileStream rawStream = new FileStream(Path.Combine(Storage.RawFolderName, fileName), FileMode.OpenOrCreate, FileAccess.Write),
                filteredStream = new FileStream(Path.Combine(Storage.FilteredFolderName, fileName), FileMode.OpenOrCreate, FileAccess.Write))
            {
                var jsonFormatter = new DataContractJsonSerializer(typeof(Storage));
                jsonFormatter.WriteObject(rawStream, RawData);
                jsonFormatter.WriteObject(filteredStream, RawData);
                MediaScannerConnection.ScanFile(this, new string[] { Storage.ApllicationDataFolder }, null, null);
            }
        }

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