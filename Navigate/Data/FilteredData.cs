using System;
using System.Runtime.Serialization;

namespace Mirea.Snar2017.Navigate
{
    [DataContract]
    public class FilteredData : DataLog<StateVector>
    {
        [DataMember]
        public RawData RawDataLog{ get; set; }

        public static FilteredData FromRawData(RawData rawData)
        {
            // TODO: Filter rawData
            throw new NotImplementedException();
        }

        public override void ToCsv(string fullFileName)
        {
            throw new NotImplementedException();
        }
    }
}