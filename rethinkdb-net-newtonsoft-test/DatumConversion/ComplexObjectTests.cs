using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.Newtonsoft.Configuration;
using RethinkDb.Newtonsoft.Converters;
using RethinkDb.Newtonsoft.Test.TestObjects;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Test.DatumConversion
{
    [TestFixture]
    public class ComplexObjectTests
    {
        [Test]
        public void serialize_complex_object()
        {
            var obj = NewObjectWithDefaults();

            var newtonDatum = DatumConvert.SerializeObject( obj, new TimeSpanConverter() );

            var truthDatum = GetDatum_InlineOrder();

            truthDatum.ShouldBeEquivalentTo( newtonDatum );
        }


        [Test]
        public void deserialize_complex_object()
        {
            var datum = GetDatum_InlineOrder();

            var newtonObject = DatumConvert.DeserializeObject<ComplexObject>( datum, ConfigurationAssembler.DefaultJsonSerializerSettings );

            var truth = NewObjectWithDefaults();

            truth.ShouldBeEquivalentTo( newtonObject );
        }

        [Test]
        public void deseralize_compelx_object_out_of_order()
        {
            var datum = GetDatum_RandomOrder();

            var newtonObject = DatumConvert.DeserializeObject<ComplexObject>( datum, ConfigurationAssembler.DefaultJsonSerializerSettings );

            var truth = NewObjectWithDefaults();

            truth.ShouldBeEquivalentTo( newtonObject );
        }

        public static ComplexObject NewObjectWithDefaults()
        {
            var obj = new ComplexObject
                {
                    Id = Guid.Parse("{32753EDC-E5EF-46E0-ABCD-CE5413B30797}"),
                    Name = "Brian Chavez",
                    ProfileUri = new Uri("http://www.bitarmory.com"),
                    CompanyUri = null,
                    Balance = 1000001.2m,
                    Clicks = 2000,
                    Views = null,
                    SecurityStamp = Guid.Parse("32753EDC-E5EF-46E0-ABCD-CE5413B30797"),
                    TrackingId = null,
                    LastLogin = new DateTime(2013, 1, 14, 4, 44, 25),
                    LoginWindow = new TimeSpan(1, 2, 3, 4, 5),
                    Signature = new byte[] {0xde, 0xad, 0xbe, 0xef},
                    Hours = new[] {1, 2, 3, 4},
                    ExtraInfo = new Dictionary<string, string>()
                        {
                            {"key1", "value1"},
                            {"key2", "value2"},
                            {"key3", "value3"},
                        },
                    Enabled = true,
                    Notify = null,
                    BinaryBools = new[] {true, false, true},
                    NullBinaryBools = new bool?[] {true, null, true}
                };
            return obj;
        }



        public Datum GetDatum_InlineOrder()
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
                    key = "ProfileUri",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "http://www.bitarmory.com"
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "CompanyUri",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NULL,
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "Clicks",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = 2000
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "Views",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NULL,
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "SecurityStamp",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = "32753edc-e5ef-46e0-abcd-ce5413b30797"
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "TrackingId",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NULL,
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "Balance",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = 1000001.2
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "Signature",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_ARRAY,
                            r_array =
                                {
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 222},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 173},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 190},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 239},

                                }
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "Hours",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_ARRAY,
                            r_array =
                                {
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 1},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 2},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 3},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 4},

                                }
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "LastLogin",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_OBJECT,
                            r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1358138665}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "+00:00"}},
                                }
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "LoginWindow",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = new TimeSpan( 1, 2, 3, 4, 5 ).TotalSeconds
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "ExtraInfo",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_OBJECT,
                            r_object =
                                {
                                    new Datum.AssocPair() {key = "key1", val = new Datum {type = Datum.DatumType.R_STR, r_str = "value1"}},
                                    new Datum.AssocPair() {key = "key2", val = new Datum {type = Datum.DatumType.R_STR, r_str = "value2"}},
                                    new Datum.AssocPair() {key = "key3", val = new Datum {type = Datum.DatumType.R_STR, r_str = "value3"}},
                                }
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "Enabled",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_BOOL,
                            r_bool = true
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "Notify",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_NULL
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "BinaryBools",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_ARRAY,
                            r_array =
                                {
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = true},
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = false},
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = true},

                                }
                        }
                } );
            truth.r_object.Add( new Datum.AssocPair
                {
                    key = "NullBinaryBools",
                    val = new Datum
                        {
                            type = Datum.DatumType.R_ARRAY,
                            r_array =
                                {
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = true},
                                    new Datum{type = Datum.DatumType.R_NULL},
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = true},

                                }
                        }
                } );

            return truth;
        }

        public Datum GetDatum_RandomOrder()
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
                key = "Views",
                val = new Datum
                {
                    type = Datum.DatumType.R_NULL,
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "TrackingId",
                val = new Datum
                {
                    type = Datum.DatumType.R_NULL,
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Signature",
                val = new Datum
                {
                    type = Datum.DatumType.R_ARRAY,
                    r_array =
                                {
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 222},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 173},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 190},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 239},

                                }
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "SecurityStamp",
                val = new Datum
                {
                    type = Datum.DatumType.R_STR,
                    r_str = "32753edc-e5ef-46e0-abcd-ce5413b30797"
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "ProfileUri",
                val = new Datum
                {
                    type = Datum.DatumType.R_STR,
                    r_str = "http://www.bitarmory.com"
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "NullBinaryBools",
                val = new Datum
                {
                    type = Datum.DatumType.R_ARRAY,
                    r_array =
                                {
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = true},
                                    new Datum{type = Datum.DatumType.R_NULL},
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = true},

                                }
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Notify",
                val = new Datum
                {
                    type = Datum.DatumType.R_NULL
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
                key = "LoginWindow",
                val = new Datum
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = new TimeSpan( 1, 2, 3, 4, 5 ).TotalSeconds
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "LastLogin",
                val = new Datum
                {
                    type = Datum.DatumType.R_OBJECT,
                    r_object =
                                {
                                    new Datum.AssocPair {key = "$reql_type$", val = new Datum {type = Datum.DatumType.R_STR, r_str = "TIME"}},
                                    new Datum.AssocPair {key = "epoch_time", val = new Datum {type = Datum.DatumType.R_NUM, r_num = 1358138665}},
                                    new Datum.AssocPair {key = "timezone", val = new Datum {type = Datum.DatumType.R_STR, r_str = "+00:00"}},
                                }
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Hours",
                val = new Datum
                {
                    type = Datum.DatumType.R_ARRAY,
                    r_array =
                                {
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 1},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 2},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 3},
                                    new Datum{type = Datum.DatumType.R_NUM, r_num = 4},

                                }
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "ExtraInfo",
                val = new Datum
                {
                    type = Datum.DatumType.R_OBJECT,
                    r_object =
                                {
                                    new Datum.AssocPair() {key = "key1", val = new Datum {type = Datum.DatumType.R_STR, r_str = "value1"}},
                                    new Datum.AssocPair() {key = "key2", val = new Datum {type = Datum.DatumType.R_STR, r_str = "value2"}},
                                    new Datum.AssocPair() {key = "key3", val = new Datum {type = Datum.DatumType.R_STR, r_str = "value3"}},
                                }
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Enabled",
                val = new Datum
                {
                    type = Datum.DatumType.R_BOOL,
                    r_bool = true
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "CompanyUri",
                val = new Datum
                {
                    type = Datum.DatumType.R_NULL,
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Clicks",
                val = new Datum
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = 2000
                }
            } );
            truth.r_object.Add( new Datum.AssocPair
            {
                key = "BinaryBools",
                val = new Datum
                {
                    type = Datum.DatumType.R_ARRAY,
                    r_array =
                                {
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = true},
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = false},
                                    new Datum{type = Datum.DatumType.R_BOOL, r_bool = true},

                                }
                }
            } );

            truth.r_object.Add( new Datum.AssocPair
            {
                key = "Balance",
                val = new Datum
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = 1000001.2
                }
            } );

            return truth;
        }
    }
}