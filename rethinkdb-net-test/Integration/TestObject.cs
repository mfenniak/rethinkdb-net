using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace RethinkDb.Test.Integration
{
    [DataContract]
    public class TestObject
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id;

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

        [DataMember(Name = "tags")]
        public string[] Tags;

        [DataMember(Name = "guid")]
        public Guid Guid;

        public override bool Equals(object obj)
        {
            var objTo = obj as TestObject;
            if (objTo != null)
                return Id != null && objTo.Id != null && String.Equals(Id, objTo.Id);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Id != null)
                return Id.GetHashCode();
            else
                return base.GetHashCode();
        }
    }
}

