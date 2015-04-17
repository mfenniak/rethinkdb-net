using System.Runtime.Serialization;

namespace RethinkDb
{
    [DataContract]
    public class DmlResponse
    {
        [DataMember(Name = "dbs_created")]
        public uint DbsCreated;

        [DataMember(Name = "dbs_dropped")]
        public uint DbsDropped;

        [DataMember(Name = "tables_created")]
        public uint TablesCreated;

        [DataMember(Name = "tables_dropped")]
        public uint TablesDropped;

        [DataMember(Name = "created")]
        public uint Created;

        [DataMember(Name = "dropped")]
        public uint Dropped;

        [DataMember(Name = "inserted")]
        public uint Inserted;

        [DataMember(Name = "updated")]
        public uint Updated;

        [DataMember(Name = "replaced")]
        public uint Replaced;

        [DataMember(Name = "deleted")]
        public uint Deleted;

        [DataMember(Name = "skipped")]
        public uint Skipped;

        [DataMember(Name = "errors")]
        public uint Errors;

        [DataMember(Name = "first_error")]
        public string FirstError;

        [DataMember(Name = "generated_keys")]
        public string[] GeneratedKeys;
    }

    [DataContract]
    public class DmlResponse<T> : DmlResponse
    {
        public DmlResponse()
        {
            Changes = new DmlResponseChange<T>[0];
        }

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
