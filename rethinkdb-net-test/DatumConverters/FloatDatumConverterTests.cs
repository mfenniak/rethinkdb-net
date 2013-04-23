using System;
using NUnit.Framework;

namespace RethinkDb.Test
{
    [TestFixture]
    public class FloatDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooLargeToRepresentAsLongProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<float>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 1.0 + float.MaxValue});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooSmallToRepresentAsLongProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<float>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = float.MinValue - 1.0});
        }

        [Test]
        public void ConvertDatum_ValueWithinRange_ReturnsValue()
        {
            const float expectedValue = 3000;
            var value = PrimitiveDatumConverterFactory.Instance.Get<float>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = expectedValue});

            Assert.AreEqual(expectedValue, value, "should be equal");
        }
    }
}

