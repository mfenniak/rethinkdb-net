using System;
using NUnit.Framework;

namespace RethinkDb.Test
{
    [TestFixture]
    public class NullableCharDatumConverterTests
    {
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_LongerThanOneCharacterString_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<char?>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = "00"});
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_ZeroCharacterString_ThrowException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<char?>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = ""});
        }

        [Test]
        public void ConvertDatum_ValueWithinRange_ReturnsValue()
        {
            const char expectedValue = '0';
            var value = PrimitiveDatumConverterFactory.Instance.Get<char?>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = expectedValue.ToString()});

            Assert.AreEqual(expectedValue, value, "should be equal");
        }
    }
}

