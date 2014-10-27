using System;

namespace RethinkDb.Newtonsoft.Test.TestObjects
{
    public class TimeSpanObject
    {
        public TimeSpanObject()
        {
            this.Id = Guid.Parse("{32753EDC-E5EF-46E0-ABCD-CE5413B30797}");
            this.Name = "Brian Chavez";

            this.TheTimeSpan = TimeSpan.FromHours(3);
            this.NullTimeSpan = null;
            this.NotNullTimeSpan = TimeSpan.FromHours(5);
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public TimeSpan TheTimeSpan { get; set; }
        public TimeSpan? NullTimeSpan { get; set; }
        public TimeSpan? NotNullTimeSpan { get; set; }
    }
}