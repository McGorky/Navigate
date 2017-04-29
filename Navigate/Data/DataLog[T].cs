using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mirea.Snar2017.Navigate
{
    // TODO: XML Serialization
    [DataContract]
    public abstract class DataLog<T>
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<T> Data { get; set; } = new List<T>();

        [IgnoreDataMember]
        public int EntriesCount { get => Data.Count; }

        public void Add(T data)
        {
            Data.Add(data);
        }

        public abstract void ToCsv(string fullFileName);
    }
}