using System;
using System.Runtime.Serialization;

namespace RethinkDb
{
    [DataContract]
    public class UngroupObject<TGroup, TReduction>
    {
        [DataMember(Name = "group")]
        public TGroup Group;

        [DataMember(Name = "reduction")]
        public TReduction Reduction;
    }
}
