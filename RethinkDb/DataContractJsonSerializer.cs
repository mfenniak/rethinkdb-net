using System.IO;
using System.Text;

namespace RethinkDb
{
    class DataContractJsonSerializer<T> : IJsonSerializer<T>
    {
        private System.Runtime.Serialization.Json.DataContractJsonSerializer dcs;

        public DataContractJsonSerializer()
        {
            dcs = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
        }

        public DataContractJsonSerializer(System.Runtime.Serialization.Json.DataContractJsonSerializer dcs)
        {
            this.dcs = dcs;
        }

        public T Deserialize(string jsonText)
        {
            var data = Encoding.UTF8.GetBytes(jsonText);
            using (var stream = new MemoryStream(data))
            {
                return (T)dcs.ReadObject(stream);
            }
        }
    }
}
