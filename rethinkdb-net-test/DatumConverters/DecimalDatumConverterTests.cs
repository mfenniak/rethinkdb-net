using System;
using NUnit.Framework;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class DecimalDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooLargeToRepresentAsLongProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<decimal>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 1.0 + (double)decimal.MaxValue});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooSmallToRepresentAsLongProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<decimal>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = (double)decimal.MinValue - 1.0});
        }

        [Test]
        public void ConvertDatum_ValueWithinRange_ReturnsValue()
        {
            const decimal expectedValue = 3000;
            var value = PrimitiveDatumConverterFactory.Instance.Get<decimal>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = (double)expectedValue});

            Assert.AreEqual(expectedValue, value, "should be equal");
        }
    }
}

