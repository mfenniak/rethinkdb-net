using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test.Integration
{
    [DataContract]
    public class TestObject2
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public uint Id;

        [DataMember(Name = "name")]
        public string Name;
    }
}

