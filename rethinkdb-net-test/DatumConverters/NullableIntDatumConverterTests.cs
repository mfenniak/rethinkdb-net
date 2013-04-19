using System;
using NUnit.Framework;

namespace RethinkDb.Test
{
    [TestFixture]
    public class NullableIntDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Get_ValueTooLarge_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<int?>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 1.0 + int.MaxValue});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Get_ValueIsFraction_ThrowsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<int?>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_NUM, r_num = 0.25});
        }
    }
}

