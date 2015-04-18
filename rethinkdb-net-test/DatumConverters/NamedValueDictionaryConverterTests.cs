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
            intDatumConverter.ConvertDatum(Arg.Any<Datum>()).Returns(args => (int)((Datum)args[0]).r_num);
            intDatumConverter.ConvertObject(Arg.Any<object>()).Returns(args => new Datum()
            {
                type = Datum.DatumType.R_NUM,
                r_num = (int)args [0]
            });

            IDatumConverter value;
            datumConverterFactory
                .TryGet(typeof(int), datumConverterFactory, out value)
                .Returns(args =>
                {
                    args[2] = intDatumConverter;
                    return true;
                });

            datumConverterFactory.GetBestNativeTypeForDatum(Arg.Any<Datum>()).Returns(typeof(int));
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
        public void ConvertDatum()
        {
            var datum = new Datum
            {
                type = Datum.DatumType.R_OBJECT,
                r_object =
                {
                    new Datum.AssocPair()
                    {
                        key = "key1",
                        val = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = 1
                        }
                    },
                    new Datum.AssocPair()
                    {
                        key = "key2",
                        val = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = 2
                        }
                    },
                }
            };

            var value = NamedValueDictionaryDatumConverterFactory.Instance.Get<Dictionary<string, object>>(datumConverterFactory).ConvertDatum(datum);

            value.Should().NotBeNull();
            value.Should().Contain("key1", 1);
            value.Should().Contain("key2", 2);
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

        [Test]
        public void ConvertDictionary()
        {
            var dict = new Dictionary<string, object>()
            {
                { "key1", 1 },
                { "key2", 2 },
            };

            var datum = NamedValueDictionaryDatumConverterFactory.Instance.Get<Dictionary<string, object>>(datumConverterFactory).ConvertObject(dict);

            datum.ShouldBeEquivalentTo(
                new Datum
                {
                    type = Datum.DatumType.R_OBJECT,
                    r_object =
                    {
                        new Datum.AssocPair()
                        {
                            key = "key1",
                            val = new Datum()
                            {
                                type = Datum.DatumType.R_NUM,
                                r_num = 1
                            }
                        },
                        new Datum.AssocPair()
                        {
                            key = "key2",
                            val = new Datum()
                            {
                                type = Datum.DatumType.R_NUM,
                                r_num = 2
                            }
                        },
                    }
                }
            );
        }
    }
}
