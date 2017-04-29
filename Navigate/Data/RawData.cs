using System;
using System.Runtime.Serialization;

using OpenTK;

namespace Mirea.Snar2017.Navigate
{
    [DataContract]
    public class RawData : DataLog<SensorsReadings>
    {
        [DataMember]
        //public Matrix3 AccelerometerCalibrationMatrix { get; set; }
        public Matrix AccelerometerCalibrationMatrix { get; set; }

        [DataMember]
        public Vector3 GyroscopeCalibrationVector { get; set; }

        public override void ToCsv(string fullFileName)
        {
            throw new NotImplementedException();
        }
    }
}