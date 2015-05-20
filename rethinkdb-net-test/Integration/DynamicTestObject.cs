using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test.Integration
{
    [DataContract]
    public class DynamicTestObject
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public Guid Id;

        [DataMember(Name = "d1")]
        public dynamic d1;

        [DataMember(Name = "d2")]
        public dynamic d2 { get; set; }

        [DataMember(Name = "d3")]
        public dynamic d3;

        [DataMember(Name = "d4")]
        public dynamic d4 { get; set; }
    }
}
