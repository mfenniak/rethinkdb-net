using System;
using NUnit.Framework;

namespace RethinkDb.Test
{
    [TestFixture]
    public class DateTimeOffsetDatumConverterTests
    {
        [Test]
        public void ConvertDatum_ValidDatum_ReturnsDateTimeOffset()
        {
            var dateOffsetString = "2013-01-01T07:00:00.000+00:00";

            var dateOffset = DateTimeOffset.Parse(dateOffsetString);

            var result = DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = dateOffsetString});

            Assert.AreEqual(dateOffset, result, "should match");
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ConvertDatum_InvalidDateTimeOffset_ThrowsException()
        {
            var dateOffsetString = "NonsenseStringThatDoesNotSerializeToDateTimeOffset";

            DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertDatum(new RethinkDb.Spec.Datum() {type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = dateOffsetString});
        }

        [Test]
        public void ConvertObject_ValidDateTimeOffset_ReturnsDatum()
        {
            var dateOffsetString = "2013-01-01T07:00:00.000+00:00";

            var dateOffset = DateTimeOffset.Parse(dateOffsetString);

            var result = DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertObject(dateOffset);

            Assert.AreEqual(dateOffsetString, result.r_str, "should match");
        }
    }
}

