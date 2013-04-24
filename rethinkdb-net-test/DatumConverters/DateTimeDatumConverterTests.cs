using System;
using NUnit.Framework;

namespace RethinkDb.Test
{
    [TestFixture]
    public class DateTimeDatumConverterTests
    {
        [Test]
        public void ConvertDatum_ValidDatum_ReturnsDateTime()
        {
            long ticks = 1249335477787;
            var dateString = string.Format(@"/Date({0})/", ticks);

            var date = new DateTime(ticks);

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
            long ticks = 1249335477787;
            var dateString = string.Format(@"/Date({0})/", ticks);

            var date = new DateTime(ticks);

            var result = DateTimeDatumConverterFactory.Instance.Get<DateTime>().ConvertObject(date);

            Assert.AreEqual(dateString, result.r_str, "should match");
        }
    }
}

