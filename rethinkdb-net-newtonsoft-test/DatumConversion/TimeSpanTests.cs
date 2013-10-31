using System;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.Newtonsoft;
using RethinkDb.Newtonsoft.Converters;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.DatumConversion
{
    [TestFixture]
    public class TimeSpanTests
    {
        [Test]
        public void should_be_able_to_serialize_timespan_to_floatseconds()
        {
            var span = new TimeSpan(days: 1, hours: 2, minutes: 3, seconds: 4, milliseconds: 5);
            var truth = new Datum()
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = span.TotalSeconds
                };

            var datum = DatumConvert.SerializeObject(span, new TimeSpanConverter());
            truth.ShouldBeEquivalentTo(datum);
        }

        [Test]
        public void should_be_able_to_deseralize_timespan_objects_from_floatseconds()
        {
            var timespan = new TimeSpan(days: 1, hours: 2, minutes: 3, seconds: 4, milliseconds: 5);
            var input = new Datum()
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = timespan.TotalSeconds
                };

            var output = DatumConvert.DeserializeObject<TimeSpan>(input, new TimeSpanConverter());
            timespan.Should().Be(output);
        }

        [Test]
        public void deseralizing_nullable_timespans_return_null()
        {
            var input = new Datum()
                {
                    type = Datum.DatumType.R_NULL,
                };

            var output = DatumConvert.DeserializeObject<TimeSpan?>(input, new TimeSpanConverter());

            output.Should().NotHaveValue();
        }

        [Test]
        public void deseralizing_null_timespans_return_default()
        {
            var input = new Datum()
                {
                    type = Datum.DatumType.R_NULL,
                };

            var output = DatumConvert.DeserializeObject<TimeSpan>( input, new TimeSpanConverter() );

            output.Should().Be(default(TimeSpan));
        }
    }
}