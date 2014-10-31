using System;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class DateTimeOffsetDatumConverterTests
    {
        // r.time(2014, 10, 30, 6, 1, 2.5, '-06:00')
        // Thu Oct 30 2014 06:01:02 GMT-06:00
        // {"$reql_type$":"TIME","epoch_time":1414670462.5,"timezone":"-06:00"}
        private static DateTimeOffset objectInLocal = new DateTimeOffset(
            new DateTime(2014, 10, 30, 6, 1, 2, 500),
            TimeSpan.FromHours(-6));
        private static Datum datumInLocal = new Datum() {
            type = Datum.DatumType.R_OBJECT,
            r_object = {
                new Datum.AssocPair() {
                    key = "$reql_type$",
                    val = new Datum() {
                        type = Datum.DatumType.R_STR,
                        r_str = "TIME"
                    }
                },
                new Datum.AssocPair() {
                    key = "epoch_time",
                    val = new Datum() {
                        type = Datum.DatumType.R_NUM,
                        r_num = 1414670462.5
                    }
                },
                new Datum.AssocPair() {
                    key = "timezone",
                    val = new Datum() {
                        type = Datum.DatumType.R_STR,
                        r_str = "-06:00"
                    }
                }
            }
        };

        // r.time(2014, 10, 30, 12, 1, 2.5, 'Z')
        // Thu Oct 30 2014 12:01:02 GMT+00:00
        // {"$reql_type$":"TIME","epoch_time":1414670462.5,"timezone":"+00:00"} 
        private static DateTimeOffset objectInUtc = new DateTimeOffset(
            new DateTime(2014, 10, 30, 12, 1, 2, 500),
            TimeSpan.Zero);
        private static Datum datumInUtc = new Datum() {
            type = Datum.DatumType.R_OBJECT,
            r_object = {
                new Datum.AssocPair() {
                    key = "$reql_type$",
                    val = new Datum() {
                        type = Datum.DatumType.R_STR,
                        r_str = "TIME"
                    }
                },
                new Datum.AssocPair() {
                    key = "epoch_time",
                    val = new Datum() {
                        type = Datum.DatumType.R_NUM,
                        r_num = 1414670462.5
                    }
                },
                new Datum.AssocPair() {
                    key = "timezone",
                    val = new Datum() {
                        type = Datum.DatumType.R_STR,
                        r_str = "+00:00"
                    }
                }
            }
        };


        [Test]
        public void ConvertDatumUtc()
        {
            Assert.That(
                DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertDatum(datumInUtc),
                Is.EqualTo(objectInUtc)
            );
        }

        [Test]
        public void ConvertDatumLocal()
        {
            Assert.That(
                DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertDatum(datumInLocal),
                Is.EqualTo(objectInLocal)
            );
        }

        [Test]
        public void ConvertObjectUtc()
        {
            DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertObject(objectInUtc)
                .ShouldBeEquivalentTo(datumInUtc);
        }

        [Test]
        public void ConvertObjectLocal()
        {
            DateTimeOffsetDatumConverterFactory.Instance.Get<DateTimeOffset>().ConvertObject(objectInLocal)
                .ShouldBeEquivalentTo(datumInLocal);
        }
    }
}
