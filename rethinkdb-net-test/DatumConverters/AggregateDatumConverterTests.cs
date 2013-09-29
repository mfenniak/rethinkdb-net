using NUnit.Framework;
using RethinkDb.Spec;
using System;
using RethinkDb;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class AggregateDatumConverterTests
    {
        [Test]
        public void ReturnsAvailableConverter_Single()
        {
            var fact = new AggregateDatumConverterFactory(PrimitiveDatumConverterFactory.Instance);
            var stringConverter = fact.Get<string>();
            Assert.That(stringConverter, Is.Not.Null);
            Assert.That(stringConverter, Is.TypeOf(typeof(RethinkDb.PrimitiveDatumConverterFactory.StringDatumConverter)));

            var intConverter = fact.Get<int>();
            Assert.That(intConverter, Is.Not.Null);
            Assert.That(intConverter, Is.TypeOf(typeof(RethinkDb.PrimitiveDatumConverterFactory.IntDatumConverter)));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ReturnsExceptionWhenNotAvaliable_Single()
        {
            var fact = new AggregateDatumConverterFactory(PrimitiveDatumConverterFactory.Instance);
            var stringConverter = fact.Get<DateTime>();
            Assert.That(stringConverter, Is.Null);
        }

        [Test]
        public void ReturnsAvailableConverter_Multiple()
        {
            var fact = new AggregateDatumConverterFactory(PrimitiveDatumConverterFactory.Instance, 
                                                          DateTimeDatumConverterFactory.Instance);

            var stringConverter = fact.Get<string>();
            Assert.That(stringConverter, Is.Not.Null);
            Assert.That(stringConverter, Is.TypeOf(typeof(PrimitiveDatumConverterFactory.StringDatumConverter)));

            var datetimeConverter = fact.Get<DateTime>();
            Assert.That(datetimeConverter, Is.Not.Null);
            Assert.That(datetimeConverter, Is.TypeOf(typeof(DateTimeDatumConverter)));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ReturnsExceptionWhenNotAvaliable_Multiple()
        {
            var fact = new AggregateDatumConverterFactory(PrimitiveDatumConverterFactory.Instance,
                                                          TupleDatumConverterFactory.Instance);
            var stringConverter = fact.Get<DateTime>();
            Assert.That(stringConverter, Is.Null);
        }
    }
}
