using System;
using NUnit.Framework;
using RethinkDb.Spec;
using RethinkDb.DatumConverters;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class TimeSpanDatumConverterTests
    {
        private IDatumConverter<TimeSpan> datumConverter;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverter = TimeSpanDatumConverterFactory.Instance.Get<TimeSpan>();
        }

        [Test]
        public void ConvertDatum()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_NUM,
                r_num = 60.123,
            };
            var ts = datumConverter.ConvertDatum(datum);
            Assert.That(ts, Is.EqualTo(TimeSpan.FromSeconds(60.123)));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatumWrongType()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_STR,
                r_str = "60",
            };
            datumConverter.ConvertDatum(datum);
        }

        [Test]
        public void ConvertObject()
        {
            var datum = datumConverter.ConvertObject(TimeSpan.FromSeconds(70.987));
            Assert.That(datum, Is.Not.Null);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(datum.r_num, Is.EqualTo(70.987));
        }
    }
}

