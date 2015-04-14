using System.Runtime.Serialization;

namespace RethinkDb
{
    [DataContract]
    public class DmlResponse
    {
        [DataMember(Name = "dbs_created")]
        public double DbsCreated;

        [DataMember(Name = "dbs_dropped")]
        public double DbsDropped;

        [DataMember(Name = "tables_created")]
        public double TablesCreated;

        [DataMember(Name = "tables_dropped")]
        public double TablesDropped;

        [DataMember(Name = "created")]
        public double Created;

        [DataMember(Name = "dropped")]
        public double Dropped;

        [DataMember(Name = "inserted")]
        public double Inserted;

        [DataMember(Name = "updated")]
        public double Updated;

        [DataMember(Name = "replaced")]
        public double Replaced;

        [DataMember(Name = "deleted")]
        public double Deleted;

        [DataMember(Name = "skipped")]
        public double Skipped;

        [DataMember(Name = "errors")]
        public double Errors;

        [DataMember(Name = "first_error")]
        public string FirstError;

        [DataMember(Name = "generated_keys")]
        public string[] GeneratedKeys;
    }

    [DataContract]
    public class DmlResponse<T> : DmlResponse
    {
        [DataMember(Name = "changes")]
        public DmlResponseChange<T>[] Changes;
    }

    [DataContract]
    public class DmlResponseChange<T>
    {
        [DataMember(Name = "old_val")]
        public T OldValue;

        [DataMember(Name = "new_val")]
        public T NewValue;
    }
}
