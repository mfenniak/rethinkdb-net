using System;
using NUnit.Framework;

namespace RethinkDb.Test
{
    [TestFixture]
    public class LongDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooLargeToRepresentAsLongProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 1.0 + long.MaxValue});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooSmallToRepresentAsLongProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = long.MinValue - 1.0});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueIsFraction_ThrowsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 0.25});
        }

        [Test]
        public void ConvertDatum_ValueWithinRange_ReturnsValue()
        {
            const long expectedValue = 3000;
            var value = PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = expectedValue});

            Assert.AreEqual(expectedValue, value, "should be equal");
        }
    }
}