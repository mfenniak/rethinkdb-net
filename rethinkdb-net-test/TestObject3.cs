using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test
{
    [DataContract]
    public class TestObject3
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public double Id;

        [DataMember]
        public int Value_Int;

        [DataMember]
        public int? Value_Int_Nullable;

        [DataMember]
        public long Value_Long;

        [DataMember]
        public long? Value_Long_Nullable;
    }
}
