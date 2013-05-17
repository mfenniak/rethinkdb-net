using NUnit.Framework;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class DataContractDatumConverterTests
    {
        private IDatumConverter<TestObject2> testObject2Converter;
        private IDatumConverter<TestObject4> testObject4Converter;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var datumConverterFactory = new AggregateDatumConverterFactory(
                PrimitiveDatumConverterFactory.Instance,
                DataContractDatumConverterFactory.Instance
            );
            testObject2Converter = datumConverterFactory.Get<TestObject2>();
            testObject4Converter = datumConverterFactory.Get<TestObject4>();
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
    }
}

