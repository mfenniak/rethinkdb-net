using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RethinkDb.Test.Integration
{
    [DataContract]
    public class TestObjectWithDictionary
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public Guid Id;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "data")]
        public Dictionary<string, object> FreeformProperties;
    }
}
