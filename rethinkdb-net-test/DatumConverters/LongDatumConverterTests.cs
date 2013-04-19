using System;
using NUnit.Framework;

namespace RethinkDb.Test
{
    [TestFixture]
    public class LongDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertObject_ValueTooLargeToRepresentAsDoubleProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertObject(LongDatumConstants.MaxValue + 1);
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertObject_ValueTooSmallToRepresentAsDoubleProperly_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<long>().ConvertObject(LongDatumConstants.MinValue - 1);
        }
    }
}