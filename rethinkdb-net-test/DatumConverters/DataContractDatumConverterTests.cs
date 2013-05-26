using NUnit.Framework;
using RethinkDb.Spec;
using RethinkDb.Test.Integration;
using NSubstitute;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    [Ignore("Crashes mono when run from command-line nunit-console.exe")]
    public class DataContractDatumConverterTests
    {
        private IDatumConverter<TestObject2> testObject2Converter;
        private IDatumConverter<TestObject4> testObject4Converter;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var datumConverterFactory = Substitute.For<IDatumConverterFactory>();

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

            testObject2Converter = DataContractDatumConverterFactory.Instance.Get<TestObject2>(datumConverterFactory);
            testObject4Converter = DataContractDatumConverterFactory.Instance.Get<TestObject4>(datumConverterFactory);

            IDatumConverter<string> value;
            datumConverterFactory
                .TryGet<string>(datumConverterFactory, out value)
                .Returns(args => {
                        args[1] = stringDatumConverter;
                        return true;
                    });
        }

        [Test]
        public void FieldDataContractConvertDatum()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_OBJECT
            };
            datum.r_object.Add(new Datum.AssocPair() {
                key = "name",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "Jackpot!",
                }
            });

            var obj = testObject2Converter.ConvertDatum(datum);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.Id, Is.EqualTo(0));
            Assert.That(obj.Name, Is.EqualTo("Jackpot!"));
        }

        [Test]
        public void FieldDataContractConvertObject()
        {
            var obj = new TestObject2() {
                Name = "Jackpot!",
            };
            var datum = testObject2Converter.ConvertObject(obj);

            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_OBJECT));
            Assert.That(datum.r_object.Count, Is.EqualTo(1));

            var pair = datum.r_object[0];
            Assert.That(pair.key, Is.EqualTo("name"));
            Assert.That(pair.val.type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(pair.val.r_str, Is.EqualTo("Jackpot!"));
        }

        [Test]
        public void PropertyDataContractConvertDatum()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_OBJECT
            };
            datum.r_object.Add(new Datum.AssocPair() {
                key = "name",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "Jackpot!",
                }
            });

            var obj = testObject4Converter.ConvertDatum(datum);
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.Id, Is.EqualTo(0));
            Assert.That(obj.Name, Is.EqualTo("Jackpot!"));
        }

        [Test]
        public void PropertyDataContractConvertObject()
        {
            var obj = new TestObject4() {
                Name = "Jackpot!",
            };
            var datum = testObject4Converter.ConvertObject(obj);

            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_OBJECT));
            Assert.That(datum.r_object.Count, Is.EqualTo(1));

            var pair = datum.r_object[0];
            Assert.That(pair.key, Is.EqualTo("name"));
            Assert.That(pair.val.type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(pair.val.r_str, Is.EqualTo("Jackpot!"));
        }

        [Test]
        public void FieldGetFieldName()
        {
            Assert.That(testObject2Converter, Is.InstanceOf(typeof(IObjectDatumConverter)));

            var objectDatumConverter = ((IObjectDatumConverter)testObject2Converter);
            var field = typeof(TestObject2).GetField("Name");

            Assert.That(objectDatumConverter.GetDatumFieldName(field), Is.EqualTo("name"));
        }

        [Test]
        public void PropertyGetFieldName()
        {
            Assert.That(testObject4Converter, Is.InstanceOf(typeof(IObjectDatumConverter)));

            var objectDatumConverter = ((IObjectDatumConverter)testObject4Converter);
            var property = typeof(TestObject4).GetProperty("Name");

            Assert.That(objectDatumConverter.GetDatumFieldName(property), Is.EqualTo("name"));
        }
    }
}

