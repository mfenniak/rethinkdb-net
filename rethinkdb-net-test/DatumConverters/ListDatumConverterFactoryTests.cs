using System;
using NUnit.Framework;
using RethinkDb.Spec;
using NSubstitute;
using RethinkDb.DatumConverters;
using System.Collections.Generic;

namespace RethinkDb.Test
{
    [TestFixture]
    public class ListDatumConverterFactoryTests
    {
        private IDatumConverterFactory rootDatumConverterFactory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var stringDatum = new Datum() {
                type = Datum.DatumType.R_STR,
                r_str = "Jackpot!",
            };
            var stringDatumConverter = Substitute.For<IDatumConverter<string>>();
            stringDatumConverter
                .ConvertObject("Jackpot!")
                .Returns(stringDatum);
            stringDatumConverter
                .ConvertDatum(Arg.Is<Datum>(d => d.type == stringDatum.type && d.r_str == stringDatum.r_str))
                .Returns("Jackpot!");
            ((IDatumConverter)stringDatumConverter)
                .ConvertObject("Jackpot!")
                .Returns(stringDatum);
            ((IDatumConverter)stringDatumConverter)
                .ConvertDatum(Arg.Is<Datum>(d => d.type == stringDatum.type && d.r_str == stringDatum.r_str))
                .Returns("Jackpot!");

            rootDatumConverterFactory = Substitute.For<IDatumConverterFactory>();
            IDatumConverter<string> _dcs;
            rootDatumConverterFactory
                .TryGet<string>(rootDatumConverterFactory, out _dcs)
                .Returns(args => {
                        Console.WriteLine("TryGet<string>");
                        args[1] = stringDatumConverter;
                        return true;
                    });
            IDatumConverter _dc;
            rootDatumConverterFactory
                .TryGet(typeof(String), rootDatumConverterFactory, out _dc)
                .Returns(args => {
                        args[2] = stringDatumConverter;
                        return true;
                    });
        }

        [Test]
        public void TestListStringConvertObject()
        {
            var listStringConverter = ListDatumConverterFactory.Instance.Get<List<string>>(rootDatumConverterFactory);
            var datum = listStringConverter.ConvertObject(new List<string>() { "Jackpot!" });
            Assert.That(datum, Is.Not.Null);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_ARRAY));
            Assert.That(datum.r_array, Has.Count.EqualTo(1));
            Assert.That(datum.r_array[0], Is.Not.Null);
            Assert.That(datum.r_array[0].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(datum.r_array[0].r_str, Is.EqualTo("Jackpot!"));
        }

        [Test]
        public void TestListStringConvertObjectNull()
        {
            var listStringConverter = ListDatumConverterFactory.Instance.Get<List<string>>(rootDatumConverterFactory);
            var datum = listStringConverter.ConvertObject(null);
            Assert.That(datum, Is.Not.Null);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NULL));
        }

        [Test]
        public void TestListStringConvertDatum()
        {
            var listStringConverter = ListDatumConverterFactory.Instance.Get<List<string>>(rootDatumConverterFactory);
            var datum = new Datum() {
                type = Datum.DatumType.R_ARRAY
            };
            datum.r_array.Add(
                new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "Jackpot!"
                }
            );
            var list = listStringConverter.ConvertDatum(datum);
            Assert.That(list, Is.Not.Null);
            Assert.That(list, Has.Count.EqualTo(1));
            Assert.That(list[0], Is.EqualTo("Jackpot!"));
        }

        [Test]
        public void TestListStringConvertDatumNull()
        {
            var listStringConverter = ListDatumConverterFactory.Instance.Get<List<string>>(rootDatumConverterFactory);
            var datum = new Datum() {
                type = Datum.DatumType.R_NULL
            };
            var list = listStringConverter.ConvertDatum(datum);
            Assert.That(list, Is.Null);
        }

        [Test]
        public void TestIListStringConvertObject()
        {
            var listStringConverter = ListDatumConverterFactory.Instance.Get<IList<string>>(rootDatumConverterFactory);
            var datum = listStringConverter.ConvertObject(new List<string>() { "Jackpot!" });
            Assert.That(datum, Is.Not.Null);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_ARRAY));
            Assert.That(datum.r_array, Has.Count.EqualTo(1));
            Assert.That(datum.r_array[0], Is.Not.Null);
            Assert.That(datum.r_array[0].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(datum.r_array[0].r_str, Is.EqualTo("Jackpot!"));
        }

        [Test]
        public void TestIListStringConvertObjectNull()
        {
            var listStringConverter = ListDatumConverterFactory.Instance.Get<IList<string>>(rootDatumConverterFactory);
            var datum = listStringConverter.ConvertObject(null);
            Assert.That(datum, Is.Not.Null);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NULL));
        }

        [Test]
        public void TestIListStringConvertDatum()
        {
            var listStringConverter = ListDatumConverterFactory.Instance.Get<IList<string>>(rootDatumConverterFactory);
            var datum = new Datum() {
                type = Datum.DatumType.R_ARRAY
            };
            datum.r_array.Add(
                new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "Jackpot!"
                }
            );
            var list = listStringConverter.ConvertDatum(datum);
            Assert.That(list, Is.Not.Null);
            Assert.That(list, Has.Count.EqualTo(1));
            Assert.That(list[0], Is.EqualTo("Jackpot!"));
        }

        [Test]
        public void TestIListStringConvertDatumNull()
        {
            var listStringConverter = ListDatumConverterFactory.Instance.Get<IList<string>>(rootDatumConverterFactory);
            var datum = new Datum() {
                type = Datum.DatumType.R_NULL
            };
            var list = listStringConverter.ConvertDatum(datum);
            Assert.That(list, Is.Null);
        }
    }
}
