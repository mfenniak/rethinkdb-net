using System;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.Newtonsoft.Test.TestObjects;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.DatumConversion
{
    [TestFixture]
    public class DateTimeOffsetTests
    {
        [Test]
        public void ser_deser_a_datetimeoffset()
        {
            var obj = new ADateTimeOffset()
                {
                    Id = "my_id",
                    TheDateTimeOffset = DateTimeOffset.Parse("10/30/2013 4:12:02 PM -07:00")
                };

            var truth = new Datum
                {
                    type = Datum.DatumType.R_OBJECT
                };
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "Id",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "my_id"
                        }
                });
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "TheDateTimeOffset",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_OBJECT,
                            r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1383149522}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "-07:00"}},
                                }
                        }
                });

            //ser test
            var ser = DatumConvert.SerializeObject(obj);
            ser.ShouldBeEquivalentTo(truth);

            //deser test
            var newtonObj = DatumConvert.DeserializeObject<ADateTimeOffset>(truth);
            newtonObj.ShouldBeEquivalentTo(obj);
        }

        [Test]
        public void ser_deser_datetimeoffsetnullable_nullset()
        {
            var obj = new DateTimeOffsetNullable()
                {
                    Id = "my_id",
                    TheDateTimeOffset = null
                };

            var truth = new Datum
                {
                    type = Datum.DatumType.R_OBJECT
                };
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "Id",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "my_id"
                        }
                });
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "TheDateTimeOffset",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NULL,
                        }
                });

            //ser test
            var ser = DatumConvert.SerializeObject(obj);
            ser.ShouldBeEquivalentTo(truth);

            //deser test
            var newtonObj = DatumConvert.DeserializeObject<DateTimeOffsetNullable>(truth);
            newtonObj.ShouldBeEquivalentTo(obj);
        }

        [Test]
        public void ser_deser_datetimeoffsetnullable_dateset()
        {
            var obj = new DateTimeOffsetNullable()
                {
                    Id = "my_id",
                    TheDateTimeOffset = DateTimeOffset.Parse("10/30/2013 4:12:02 PM -07:00")
                };

            var truth = new Datum
                {
                    type = Datum.DatumType.R_OBJECT
                };
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "Id",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "my_id"
                        }
                });
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "TheDateTimeOffset",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_OBJECT,
                            r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1383149522}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "-07:00"}},
                                }
                        }
                });

            //ser test
            var ser = DatumConvert.SerializeObject(obj);
            ser.ShouldBeEquivalentTo(truth);

            //deser test
            var newtonObj = DatumConvert.DeserializeObject<DateTimeOffsetNullable>(truth);
            newtonObj.ShouldBeEquivalentTo(obj);
        }
    }
}