using System;
using NUnit.Framework;

namespace RethinkDb.Test
{
    [TestFixture]
    public class UnsignedIntDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooLarge_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<uint>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 1.0 + uint.MaxValue});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooSmall_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<uint>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = uint.MinValue - 1.0});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueIsFraction_ThrowsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<uint>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 0.25});
        }

        [Test]
        public void ConvertDatum_ValueWithinRange_ReturnsValue()
        {
            const ushort expectedValue = 3000;
            var value = PrimitiveDatumConverterFactory.Instance.Get<uint>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = expectedValue});

            Assert.AreEqual(expectedValue, value, "should be equal");
        }
    }
}

