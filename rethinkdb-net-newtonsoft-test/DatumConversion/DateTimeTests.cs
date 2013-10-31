using System;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.DatumConverters;
using RethinkDb.Newtonsoft.Test.TestObjects;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.DatumConversion
{
    [TestFixture]
    public class DateTimeTests
    {
        [Test]
        [Explicit]
        public void debug()
        {
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            DateTimeDatumConverter.Instance.Value
                .ConvertObject(now)
                .ToConsoleDebug();

            Console.WriteLine("-----------");

            DateTimeDatumConverter.Instance.Value
                .ConvertObject( utcNow )
                .ToConsoleDebug();

            //Foffsets

            Console.WriteLine( "----------- OFFSETS BELOW" );


            var onow = DateTimeOffset.Now;
            var oUtcNow = DateTimeOffset.UtcNow;

            DateTimeOffsetDatumConverter.Instance.Value
                .ConvertObject( onow )
                .ToConsoleDebug();

            Console.WriteLine( "-----------" );

            DateTimeOffsetDatumConverter.Instance.Value
                .ConvertObject( oUtcNow )
                .ToConsoleDebug();
        }

        [Test]
        public void ser_deser_a_datetime()
        {
            var obj = new ADateTime
                {
                    Id = "my_id_value",
                    TheDate = DateTime.Parse("10/30/2013 4:55 PM")
                };

            var truth = new Datum
            {
                type = Datum.DatumType.R_OBJECT
            };
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Id",
                val = new Datum
                {
                    type = Datum.DatumType.R_STR,
                    r_str = "my_id_value"
                }
            } );
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "TheDate",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_OBJECT,
                            r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1383152100}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "+00:00"}},
                                }
                        }
                });

            var newtonDatum = DatumConvert.SerializeObject(obj);

            truth.ShouldBeEquivalentTo(newtonDatum);

            var newtonObject = DatumConvert.DeserializeObject<ADateTime>(truth);

            newtonObject.ShouldBeEquivalentTo(obj);
        }


        [Test]
        public void ser_deser_datetimenullable_nullset()
        {
            var obj = new DateTimeNullable()
                {
                    Id = "my_id",
                    NullableDateTime = null
                };

            var truth = new Datum
                {
                    type = Datum.DatumType.R_OBJECT
                };
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Id",
                val = new Datum
                {
                    type = Datum.DatumType.R_STR,
                    r_str = "my_id"
                }
            } );
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "NullableDateTime",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NULL,
                        }
                });

            //ser test
            var ser = DatumConvert.SerializeObject(obj);
            ser.ShouldBeEquivalentTo(truth);

            //deser test
            var newtonObj = DatumConvert.DeserializeObject<DateTimeNullable>(truth);
            newtonObj.ShouldBeEquivalentTo(obj);
        }

        [Test]
        public void ser_deser_datetimenullable_dateset()
        {
            var obj = new DateTimeNullable()
            {
                Id = "my_id",
                NullableDateTime = DateTime.Parse("10/30/2013 4:55 PM")
            };

            var truth = new Datum
            {
                type = Datum.DatumType.R_OBJECT
            };
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Id",
                val = new Datum
                {
                    type = Datum.DatumType.R_STR,
                    r_str = "my_id"
                }
            } );
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "NullableDateTime",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_OBJECT,
                            r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1383152100}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "+00:00"}},
                                }
                        }
                });

            //ser test
            var ser = DatumConvert.SerializeObject( obj );
            ser.ShouldBeEquivalentTo( truth );

            //deser test
            var newtonObj = DatumConvert.DeserializeObject<DateTimeNullable>( truth );
            newtonObj.ShouldBeEquivalentTo( obj );
        }


    }
}