using System;
using NUnit.Framework;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class DoubleDatumConverterTests
    {
        [Test]
        public void ConvertDatum_ValueWithinRange_ReturnsValue()
        {
            const double expectedValue = 3000;
            var value = PrimitiveDatumConverterFactory.Instance.Get<double>().ConvertDatum(expectedValue.ToDatum());
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ConvertDatum_ValueWithinRangeWithDecimal_ReturnsValue()
        {
            const double expectedValue = 3000.23;
            var value = PrimitiveDatumConverterFactory.Instance.Get<double>().ConvertDatum(expectedValue.ToDatum());
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_NullReturnsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<double>().ConvertDatum(new Datum { type = Datum.DatumType.R_NULL });
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_UnsupportedTypeReturnsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<double>().ConvertDatum(new Datum { type = Datum.DatumType.R_STR });
        }

        [Test]
        public void ConvertObject()
        {
            const double expectedValue = 3000;
            var obj = PrimitiveDatumConverterFactory.Instance.Get<double>().ConvertObject(expectedValue);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(obj.r_num, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ConvertObject_WithDecimal()
        {
            const double expectedValue = 3000.32;
            var obj = PrimitiveDatumConverterFactory.Instance.Get<double>().ConvertObject(expectedValue);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(obj.r_num, Is.EqualTo(expectedValue));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertObject_Null_ThrowsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<double>().ConvertObject(null);
        }
    }
}
