using System;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.Newtonsoft.Converters;
using RethinkDb.Newtonsoft.Test.TestObjects;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.DatumConversion
{
    [TestFixture]
    public class FatDateTimeTests
    {
        [Test]
        public void datum_to_datetime_test()
        {
            var datum = new Datum()
                {
                    type = Datum.DatumType.R_OBJECT
                };
            datum.r_object.Add(new Datum.AssocPair()
                {
                    key = "$reql_type$",
                    val = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "TIME"
                        }
                });
            datum.r_object.Add(new Datum.AssocPair()
                {
                    key = "epoch_time",
                    val = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = 1376704156.123
                        }
                });
            datum.r_object.Add(new Datum.AssocPair()
                {
                    key = "timezone",
                    val = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "+00:00"
                        }
                });

            var dateTime = DatumConvert.DeserializeObject<DateTime>( datum );

            dateTime.Should().Be(new DateTime(2013, 8, 17, 1, 49, 16, 123));
        }

        [Test]
        public void datum_to_datetimeoffset_test()
        {
            var datum = new Datum()
                {
                    type = Datum.DatumType.R_OBJECT
                };
            datum.r_object.Add( new Datum.AssocPair()
                {
                    key = "$reql_type$",
                    val = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "TIME"
                        }
                } );
            datum.r_object.Add( new Datum.AssocPair()
                {
                    key = "epoch_time",
                    val = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = 1376704156.123
                        }
                } );
            datum.r_object.Add( new Datum.AssocPair()
                {
                    key = "timezone",
                    val = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "-03:30"
                        }
                } );

            var dateTimeOffset = DatumConvert.DeserializeObject<DateTimeOffset>( datum );

            dateTimeOffset.Should().Be( new DateTimeOffset( new DateTime( 2013, 8, 17, 1, 49, 16, 123 ), -( new TimeSpan( 3, 30, 0 ) ) ) );

        }

        [Test]
        public void serialize_datetimeobject()
        {
            var obj = GetDefaultObject();
            var datum = DatumConvert.SerializeObject(obj);
            var truth = GetDefaultDatum();
            truth.ShouldBeEquivalentTo(datum);
        }

        [Test]
        public void deserialize_datetime_example()
        {
            var input = GetDefaultDatum();

            var result = DatumConvert.DeserializeObject<FatDateTimeObject>(input);

            var obj = GetDefaultObject();

            obj.ShouldBeEquivalentTo(result);
        }

        private FatDateTimeObject GetDefaultObject()
        {
            var obj = new FatDateTimeObject
                {
                    Id = Guid.Parse("{32753EDC-E5EF-46E0-ABCD-CE5413B30797}"),
                    Name = "Brian Chavez",
                    TheDateTime = DateTime.Parse("10/26/2013 3:45 PM"),
                    NullDateTime = null,
                    NotNullDateTime = DateTime.Parse("10/26/2013 3:45 PM"),
                    TheDateTimeOffset = DateTimeOffset.Parse("10/26/2013 8:27:02 PM -07:00"),
                    NullDateTimeOffset = null,
                    NotNullDateTimeOffset = DateTimeOffset.Parse("10/26/2013 8:27:02 PM -07:00")
                };

            return obj;
        }

        private Datum GetDefaultDatum()
        {
            //the truth datum the constructor represents.
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
                    r_str = "32753EDC-E5EF-46E0-ABCD-CE5413B30797".ToLower()
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Name",
                val = new Datum
                {
                    type = Datum.DatumType.R_STR,
                    r_str = "Brian Chavez"
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "TheDateTime",
                val = new Datum
                {
                    type = Datum.DatumType.R_OBJECT,
                    r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1382802300}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "+00:00"}},
                                }
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "NullDateTime",
                val = new Datum
                {
                    type = Datum.DatumType.R_NULL,
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "NotNullDateTime",
                val = new Datum
                {
                    type = Datum.DatumType.R_OBJECT,
                    r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1382802300}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "+00:00"}},
                                }
                }
            } );

            truth.r_object.Add( new Datum.AssocPair
            {
                key = "TheDateTimeOffset",
                val = new Datum
                {
                    type = Datum.DatumType.R_OBJECT,
                    r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1382819222}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "-07:00"}},
                                }
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "NullDateTimeOffset",
                val = new Datum
                {
                    type = Datum.DatumType.R_NULL,
                }
            } );
            truth.r_object.Add(new Datum.AssocPair
                {
                    key = "NotNullDateTimeOffset",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_OBJECT,
                            r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1382819222}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "-07:00"}},
                                }
                        }
                });
            return truth;
        }
    }
}