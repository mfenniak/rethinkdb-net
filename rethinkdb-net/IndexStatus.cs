using System.Runtime.Serialization;

namespace RethinkDb
{
    [DataContract]
    public class IndexStatus
    {
        [DataMember(Name = "index")]
        public string IndexName;

        [DataMember(Name = "ready")]
        public bool Ready;

        [DataMember(Name = "multi")]
        public bool MultiIndex;

        [DataMember(Name = "geo")]
        public bool Geographic;

        [DataMember(Name = "outdated")]
        public bool Outdated;

        [DataMember(Name = "blocks_processed")]
        public int? BlocksProcessed;

        [DataMember(Name = "blocks_total")]
        public int? BlocksTotal;
    }
}
