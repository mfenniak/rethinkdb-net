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
            PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertDatum(new RethinkDb.Spec.Datum(){r_num = 1.0 + long.MaxValue});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueTooSmallToRepresentAsLongProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertDatum(new RethinkDb.Spec.Datum(){r_num = long.MinValue - 1.0});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ValueIsFraction_ThrowsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertDatum(new RethinkDb.Spec.Datum(){r_num = 0.25});
        }
    }
}