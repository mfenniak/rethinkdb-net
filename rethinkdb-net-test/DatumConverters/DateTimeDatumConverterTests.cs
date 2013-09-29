using System;
using System.Linq;
using NUnit.Framework;
using RethinkDb.Spec;
using RethinkDb.DatumConverters;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class DateTimeDatumConverterTests
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
                    r_str = "TIME"
                }
            });
            datum.r_object.Add(new Datum.AssocPair() {
                key = "epoch_time",
                val = new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = 1376704156.123
                }
            });
            datum.r_object.Add(new Datum.AssocPair() {
                key = "timezone",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "+00:00"
                }
            });

            var result = DateTimeDatumConverterFactory.Instance.Get<DateTime>().ConvertDatum(datum);
            Assert.That(result, Is.EqualTo(new DateTime(2013, 8, 17, 1, 49, 16, 123)));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertDatum_Timezone_ThrowsException()
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_OBJECT
            };
            datum.r_object.Add(new Datum.AssocPair() {
                key = "$reql_type$",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "TIME"
                }
            });
            datum.r_object.Add(new Datum.AssocPair() {
                key = "epoch_time",
                val = new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = 1376704156.123
                }
            });
            datum.r_object.Add(new Datum.AssocPair() {
                key = "timezone",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "+01:00"
                }
            });
            DateTimeDatumConverterFactory.Instance.Get<DateTime>().ConvertDatum(datum);
        }

        [Test]
        public void ConvertObject_ValidDateTime_ReturnsDatum()
        {
            var datetime = new DateTime(2013, 8, 17, 1, 49, 16, 123);
            var datum = DateTimeDatumConverterFactory.Instance.Get<DateTime>().ConvertObject(datetime);

            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_OBJECT));
            var keys = datum.r_object.ToDictionary(kvp => kvp.key, kvp => kvp.val);

            Assert.That(keys["$reql_type$"].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(keys["$reql_type$"].r_str, Is.EqualTo("TIME"));

            Assert.That(keys["epoch_time"].type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(keys["epoch_time"].r_num, Is.EqualTo(1376704156.123));

            Assert.That(keys["timezone"].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(keys["timezone"].r_str, Is.EqualTo("+00:00"));
        }
    }
}

