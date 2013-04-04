using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test
{
    [DataContract]
    public class TestObject2
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public double Id;

        [DataMember(Name = "name")]
        public string Name;
    }
}

