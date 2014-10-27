using System;
using System.Linq;
using NUnit.Framework;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class BinaryDatumConverterTests
    {
        [Test]
        public void ConvertDatum()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_OBJECT
            };
            datum.r_object.Add(new Datum.AssocPair() {
                key = "$reql_type$",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "BINARY"
                }
            });
            datum.r_object.Add(new Datum.AssocPair() {
                key = "data",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "SGVsbG8sIHdvcmxkIQ=="
                }
            });

            var result = BinaryDatumConverterFactory.Instance.Get<byte[]>().ConvertDatum(datum);
            Assert.That(result, Is.EquivalentTo(new byte[] {
                72, 101, 108, 108, 111, 44, 32, 119, 111, 114, 108, 100, 33
            }));
        }

        [Test]
        public void ConvertObject()
        {
            var data = new byte[]
            {
                72, 101, 108, 108, 111, 44, 32, 0, 119, 111, 114, 108, 100, 33
            };
            var datum = BinaryDatumConverterFactory.Instance.Get<byte[]>().ConvertObject(data);

            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_OBJECT));
            var keys = datum.r_object.ToDictionary(kvp => kvp.key, kvp => kvp.val);

            Assert.That(keys["$reql_type$"].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(keys["$reql_type$"].r_str, Is.EqualTo("BINARY"));

            Assert.That(keys["data"].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(keys["data"].r_str, Is.EqualTo("SGVsbG8sIAB3b3JsZCE="));
        }
    }
}
