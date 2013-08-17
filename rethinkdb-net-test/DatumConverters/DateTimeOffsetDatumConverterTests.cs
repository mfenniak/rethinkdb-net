using System;
using System.Linq;
using NUnit.Framework;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class DateTimeOffsetDatumConverterTests
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
                    r_str = "-03:30"
                }
            });

            var result = DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertDatum(datum);
            Assert.That(result, Is.EqualTo(new DateTimeOffset(new DateTime(2013, 8, 17, 1, 49, 16, 123), -(new TimeSpan(3, 30, 0)))));
        }

        [Test]
        public void ConvertObject_ValidDateTime_ReturnsDatum()
        {
            var datetime = new DateTimeOffset(new DateTime(2013, 8, 17, 1, 49, 16, 123), -(new TimeSpan(3, 30, 0)));
            var datum = DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertObject(datetime);

            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_OBJECT));
            var keys = datum.r_object.ToDictionary(kvp => kvp.key, kvp => kvp.val);

            Assert.That(keys["$reql_type$"].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(keys["$reql_type$"].r_str, Is.EqualTo("TIME"));

            Assert.That(keys["epoch_time"].type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(keys["epoch_time"].r_num, Is.EqualTo(1376704156.123));

            Assert.That(keys["timezone"].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(keys["timezone"].r_str, Is.EqualTo("-03:30"));
        }
    }
}

