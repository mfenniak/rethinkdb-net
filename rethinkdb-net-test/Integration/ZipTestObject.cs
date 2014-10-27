using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace RethinkDb.Test.Integration
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

        [DataMember(Name = "childrenList")]
        public List<TestObject> ChildrenList;

        [DataMember(Name = "childrenListInterface")]
        public IList<TestObject> ChildrenIList;

        [DataMember(Name = "number")]
        public double SomeNumber;
    }
}

