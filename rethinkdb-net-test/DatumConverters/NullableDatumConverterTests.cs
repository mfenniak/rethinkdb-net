using System;
using NUnit.Framework;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class NullableDatumConverterTests
    {
        private IDatumConverter<int?> datumConverter;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverter = new AggregateDatumConverterFactory(
                PrimitiveDatumConverterFactory.Instance,
                NullableDatumConverterFactory.Instance
            ).Get<int?>();
        }

        [Test]
        public void ConvertObject()
        {
            var datum = datumConverter.ConvertObject(5);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NUM));
        }

        [Test]
        public void ConvertObjectNull()
        {
            var datum = datumConverter.ConvertObject(null);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NULL));
        }

        [Test]
        public void NonGenericConvertObject()
        {
            var datum = ((IDatumConverter)datumConverter).ConvertObject(5);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NUM));
        }

        [Test]
        public void NonGenericConvertObjectNull()
        {
            var datum = ((IDatumConverter)datumConverter).ConvertObject(null);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NULL));
        }

        [Test]
        public void ConvertDatum()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_NUM,
                r_num = 5
            };
            var value = datumConverter.ConvertDatum(datum);
            Assert.That(value.HasValue);
        }

        [Test]
        public void ConvertDatumNullable()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_NULL,
            };
            var value = datumConverter.ConvertDatum(datum);
            Assert.That(value.HasValue, Is.False);
        }

        [Test]
        public void NonGenericConvertDatum()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_NUM,
                r_num = 5
            };
            var value = ((IDatumConverter)datumConverter).ConvertDatum(datum);
            Assert.That(value, Is.EqualTo(5));
        }

        [Test]
        public void NonGenericConvertDatumNullable()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_NULL,
            };
            var value = ((IDatumConverter)datumConverter).ConvertDatum(datum);
            Assert.That(value, Is.Null);
        }
    }
}

