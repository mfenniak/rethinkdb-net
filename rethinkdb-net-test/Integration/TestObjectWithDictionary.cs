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

        [DataMember(Name = "valuetypedata")]
        public Dictionary<string, int> IntegerProperties;

        [DataMember(Name = "referencetypedata")]
        public Dictionary<string, string> StringProperties;

    }
}
