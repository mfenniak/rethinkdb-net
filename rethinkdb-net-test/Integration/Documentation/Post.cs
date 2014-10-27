using System;
using System.Runtime.Serialization;

namespace RethinkDb.Test.Integration.Documentation
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

