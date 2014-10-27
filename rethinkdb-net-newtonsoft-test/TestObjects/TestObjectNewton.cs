using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RethinkDb.Test.Integration;

namespace RethinkDb.Newtonsoft.Test.TestObjects
{
    //TestObject model copied from RethinkDb.Test.Integration.TestObject.cs
    public class TestObjectNewton
    {
        //[DataMember( Name = "id", EmitDefaultValue = false )]
        //[JsonProperty("id")]
        public string Id { get; set; }

        //[DataMember( Name = "name" )]
        public string Name { get; set; }

        //[DataMember( Name = "children" )]
        public TestObject[] Children { get; set; }

        //[DataMember( Name = "childrenList" )]
        public List<TestObject> ChildrenList { get; set; }

        //[DataMember( Name = "childrenListInterface" )]
        [JsonProperty("childrenListInterface")]
        public IList<TestObject> ChildrenIList { get; set; }

        //[DataMember( Name = "number" )]
        [JsonProperty("number")]
        public double SomeNumber { get; set; }

        //[DataMember( Name = "tags" )]
        public string[] Tags { get; set; }

        //[DataMember(Name = "guid")]
        public Guid Guid { get; set; }
    }
}
