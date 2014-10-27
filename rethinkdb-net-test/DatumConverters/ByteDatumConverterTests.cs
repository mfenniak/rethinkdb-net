using System;
using NUnit.Framework;
using RethinkDb.DatumConverters;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class ByteDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooLarge_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<byte>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 1.0 + byte.MaxValue});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooSmall_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<byte>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = byte.MinValue - 1.0});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueIsFraction_ThrowsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<byte>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 0.25});
        }

        [Test]
        public void ConvertDatum_ValueWithinRange_ReturnsValue()
        {
            const byte expectedValue = 6;
            var value = PrimitiveDatumConverterFactory.Instance.Get<byte>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = expectedValue});

            Assert.AreEqual(expectedValue, value, "should be equal");
        }
    }
}

