using System;
using NUnit.Framework;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class DateTimeDatumConverterTests
    {
        [Test]
        public void ConvertDatum_ValidDatum_ReturnsDateTime()
        {
            var dateString = "2013-01-01T15:00:00.000Z";

            var date = DateTime.Parse(dateString);

            var result = DateTimeDatumConverterFactory.Instance.Get<DateTime>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = dateString});

            Assert.AreEqual(date, result, "should match");
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ConvertDatum_InvalidDateTime_ThrowsException()
        {
            var dateString = "NonsenseStringThatDoesNotSerializeToDateTime";

            DateTimeDatumConverterFactory.Instance.Get<DateTime>().ConvertDatum(new RethinkDb.Spec.Datum() {type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = dateString});
        }

        [Test]
        public void ConvertObject_ValidDateTime_ReturnsDatum()
        {
            var dateString = "2013-01-01T15:00:00.000Z";

            var date = DateTime.Parse(dateString);

            var result = DateTimeDatumConverterFactory.Instance.Get<DateTime>().ConvertObject(date);

            Assert.AreEqual(dateString, result.r_str, "should match");
        }
    }
}

