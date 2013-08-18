using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test
{
    [DataContract]
    public class Post
    {
        [DataMember]
        public string Title;

        [DataMember]
        public string Content;
    }
}

