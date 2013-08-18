using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test
{
    [DataContract]
    public class Author
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id;

        [DataMember]
        public string Name;

        [DataMember]
        public string TVShow;

        [DataMember]
        public string Type;

        [DataMember]
        public string Rank;

        [DataMember]
        public Post[] Posts;
    }
}

