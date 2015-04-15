using System.Runtime.Serialization;

namespace RethinkDb
{
    [DataContract]
    public class MatchResponse
    {
        [DataMember(Name = "start")]
        public uint Start;

        [DataMember(Name = "end")]
        public uint End;

        [DataMember(Name = "str")]
        public string MatchedString;

        [DataMember(Name = "groups")]
        public MatchGroupResponse[] Groups;
    }
}
