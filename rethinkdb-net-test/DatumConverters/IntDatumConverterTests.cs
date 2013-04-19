using System;
using NUnit.Framework;
using ProtoBuf;

namespace RethinkDb.Test
{
    [TestFixture]
    public class IntDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Get_ValueTooLarge_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<int>().ConvertDatum(new RethinkDb.Spec.Datum(){r_num = 1.0 + int.MaxValue});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Get_ValueIsFraction_ThrowsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<int>().ConvertDatum(new RethinkDb.Spec.Datum(){r_num = 0.25});
        }
    }
}

