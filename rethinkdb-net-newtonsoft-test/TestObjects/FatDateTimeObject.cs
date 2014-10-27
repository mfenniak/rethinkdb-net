using System;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.TestObjects
{
    public class FatDateTimeObject
    {
        public FatDateTimeObject()
        {
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public DateTime TheDateTime { get; set; }
        public DateTime? NullDateTime { get; set; }
        public DateTime? NotNullDateTime { get; set; }

        public DateTimeOffset TheDateTimeOffset { get; set; }
        public DateTimeOffset? NullDateTimeOffset { get; set; }
        public DateTimeOffset? NotNullDateTimeOffset { get; set; }
    }
}