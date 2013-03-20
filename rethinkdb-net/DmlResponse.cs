using System;
using System.Runtime.Serialization;

namespace RethinkDb
{
    [DataContract]
    public class DmlResponse
    {
        [DataMember(Name = "created")]
        public int Created;

        [DataMember(Name = "dropped")]
        public int Dropped;

        [DataMember(Name = "inserted")]
        public int Inserted;

        [DataMember(Name = "updated")]
        public int Updated;

        [DataMember(Name = "deleted")]
        public int Deleted;

        [DataMember(Name = "skipped")]
        public int Skipped;

        [DataMember(Name = "errors")]
        public int Errors;

        [DataMember(Name = "first_error")]
        public string FirstError;

        [DataMember(Name = "generated_keys")]
        public string[] GeneratedKeys;
    }
}

