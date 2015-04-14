using System.Runtime.Serialization;

namespace RethinkDb.Test.Integration
{
    [DataContract]
    public class TestObject4
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public uint Id
        {
            get;
            set;
        }

        [DataMember(Name = "name")]
        public string Name
        {
            get;
            set;
        }
    }
}
