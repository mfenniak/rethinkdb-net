using System;
using System.Security;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.TestObjects
{
    public partial class ADateTime
    {
        public string Id { get; set; }
        public DateTime TheDate { get; set; }
    }
}
