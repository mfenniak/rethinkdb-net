using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test
{
    [DataContract]
    public class ZipTestObject
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id;

        [DataMember(Name = "first_name")]
        public string FirstName;

        [DataMember(Name = "last_name")]
        public string LastName;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "children")]
        public TestObject[] Children;

        [DataMember(Name = "number")]
        public double SomeNumber;
    }
}

