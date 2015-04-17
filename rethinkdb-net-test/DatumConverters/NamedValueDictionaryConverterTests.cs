using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RethinkDb.Spec;
using NSubstitute;
using RethinkDb.DatumConverters;
using FluentAssertions;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class NamedValueDictionaryConverterTests
    {
        private IDatumConverterFactory datumConverterFactory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            datumConverterFactory = Substitute.For<IDatumConverterFactory>();

            var intDatumConverter = Substitute.For<IDatumConverter>();

            IDatumConverter value;
            datumConverterFactory
                .TryGet(typeof(int), datumConverterFactory, out value)
                .Returns(args =>
                {
                    args[2] = intDatumConverter;
                    return true;
                });
        }

        [Test]
        public void ConvertDatumEmpty()
        {
            var datum = new Datum { type = Datum.DatumType.R_OBJECT };

            var value = NamedValueDictionaryDatumConverterFactory.Instance.Get<Dictionary<string, object>>(datumConverterFactory).ConvertDatum(datum);

            value.Should().NotBeNull();
            value.Count.Should().Be(0);
        }

        [Test]
        public void ConvertDictionaryEmpty()
        {
            var value = new Dictionary<string, object>();

            var datum = NamedValueDictionaryDatumConverterFactory.Instance.Get<Dictionary<string, object>>(datumConverterFactory).ConvertObject(value);

            datum.Should().NotBeNull();
            datum.type.Should().Be(Datum.DatumType.R_OBJECT);
            datum.r_object.Should().NotBeNull();
            datum.r_object.Should().HaveCount(0);
        }


    }
}
