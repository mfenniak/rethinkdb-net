using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.TestObjects
{
    public class ComplexObject
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        public string Name { get; set; }
        public Uri ProfileUri { get; set; }
        public Uri CompanyUri { get; set; }
        public int? Clicks { get; set; }
        public int? Views { get; set; }
        public Guid? SecurityStamp { get; set; }
        public Guid? TrackingId { get; set; }
        public decimal? Balance { get; set; }
        public byte[] Signature { get; set; }
        public int[] Hours { get; set; }
        public DateTime LastLogin { get; set; }
        public TimeSpan LoginWindow { get; set; }
        public Dictionary<string, string> ExtraInfo { get; set; }
        public bool Enabled { get; set; }
        public bool? Notify { get; set; }
        public bool[] BinaryBools { get; set; }
        public bool?[] NullBinaryBools { get; set; }

        public double SomeNumber { get; set; }
    }
}