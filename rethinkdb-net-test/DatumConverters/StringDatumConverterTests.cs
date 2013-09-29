using System;
using NUnit.Framework;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class StringDatumConverterTests
    {
        [Test]
        public void ConvertDatum_Null()
        {
            const string expectedValue = null;
            var value = PrimitiveDatumConverterFactory.Instance.Get<string>().ConvertDatum(expectedValue.ToDatum());
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_UnsupportedTypeReturnsException()
        {
            PrimitiveDatumConverterFactory.Instance.Get<string>().ConvertDatum(new Datum { type = Datum.DatumType.R_ARRAY });
        }

        [Test]
        public void ConvertDatum_EmptyString()
        {
            const string expectedValue = "";
            var value = PrimitiveDatumConverterFactory.Instance.Get<string>().ConvertDatum(expectedValue.ToDatum());
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ConvertDatum_Unicode()
        {
            const string expectedValue = "؁";
            var value = PrimitiveDatumConverterFactory.Instance.Get<string>().ConvertDatum(expectedValue.ToDatum());
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ConvertDatum_ReturnsValue()
        {
            const string expectedValue = "loremipsum";
            var value = PrimitiveDatumConverterFactory.Instance.Get<string>().ConvertDatum(expectedValue.ToDatum());
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ConvertObject()
        {
            const string expectedValue = "loremipsum";
            var obj = PrimitiveDatumConverterFactory.Instance.Get<string>().ConvertObject(expectedValue);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(obj.r_str, Is.EqualTo(expectedValue));
        }

        [Test]
        public void ConvertObject_Null()
        {
            var obj = PrimitiveDatumConverterFactory.Instance.Get<string>().ConvertObject(null);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.type, Is.EqualTo(Datum.DatumType.R_NULL));
        }
    }
}

