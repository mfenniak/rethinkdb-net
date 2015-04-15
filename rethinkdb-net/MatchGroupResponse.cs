using System.Runtime.Serialization;

namespace RethinkDb
{
    [DataContract]
    public class MatchGroupResponse
    {
        [DataMember(Name = "start")]
        public uint Start;

        [DataMember(Name = "end")]
        public uint End;

        [DataMember(Name = "str")]
        public string MatchedString;
    }
}
