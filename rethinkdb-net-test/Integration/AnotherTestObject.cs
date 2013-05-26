using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test.Integration
{
    [DataContract]
    public class AnotherTestObject
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id;

        [DataMember(Name = "first_name")]
        public string FirstName;

        [DataMember(Name = "last_name")]
        public string LastName;

        public override bool Equals(object obj)
        {
            var objTo = obj as AnotherTestObject;
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

