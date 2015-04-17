using System;
using NUnit.Framework;
using NSubstitute;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;
using FluentAssertions;
using System.Collections.Generic;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class AbstractDatumConverterFactoryTests
    {
        private static readonly Datum dateTimeOffsetDatum = new Datum()
        {
            type = Datum.DatumType.R_OBJECT,
            r_object =
            {
                new Datum.AssocPair()
                {
                    key = "$reql_type$",
                    val = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = "TIME"
                    }
                },
                new Datum.AssocPair()
                {
                    key = "epoch_time",
                    val = new Datum()
                    {
                        type = Datum.DatumType.R_NUM,
                        r_num = 1414670462.5
                    }
                },
                new Datum.AssocPair()
                {
                    key = "timezone",
                    val = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = "-06:00"
                    }
                }
            }
        };

        public class TestDatumConverterFactory : AbstractDatumConverterFactory
        {
            public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
            {
                if (typeof(T) == typeof(int))
                {
                    datumConverter = Substitute.For<IDatumConverter<T>>();
                    return true;
                }
                else
                {
                    datumConverter = null;
                    return false;
                }
            }
        }

        [Test]
        public void BaselineGenericTryGet()
        {
            var dcf = (IDatumConverterFactory)new TestDatumConverterFactory();
            IDatumConverter<int> dc;
            Assert.That(dcf.TryGet<int>(dcf, out dc), Is.True);
            Assert.That(dc, Is.Not.Null);
        }

        [Test]
        public void NonGenericTryGetSuccess()
        {
            var dcf = (IDatumConverterFactory)new TestDatumConverterFactory();
            IDatumConverter dc;
            Assert.That(dcf.TryGet(typeof(int), dcf, out dc), Is.True);
            Assert.That(dc, Is.Not.Null);
        }

        [Test]
        public void NonGenericTryGetFailure()
        {
            var dcf = (IDatumConverterFactory)new TestDatumConverterFactory();
            IDatumConverter dc;
            Assert.That(dcf.TryGet(typeof(string), dcf, out dc), Is.False);
            Assert.That(dc, Is.Null);
        }

        [Test]
        public void NativeTypeNull()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                                 new Datum() { type = Datum.DatumType.R_NULL });
            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(object));
        }

        [Test]
        public void NativeTypeString()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum() { type = Datum.DatumType.R_STR, r_str = "Woot!" });
            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(string));
        }

        [Test]
        public void NativeTypeInt()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum() { type = Datum.DatumType.R_NUM, r_num = 5000 });
            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(int));
        }

        [Test]
        public void NativeTypeDouble()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum() { type = Datum.DatumType.R_NUM, r_num = 5.5 });
            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(double));
        }

        [Test]
        public void NativeTypeBinary()
        {
            var dcf = new TestDatumConverterFactory();

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
            var nativeType = dcf.GetBestNativeTypeForDatum(datum);
            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(byte[]));
        }

        [Test]
        public void NativeTypeDateTime()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(dateTimeOffsetDatum);
            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(DateTimeOffset));
        }

        [Test]
        public void NativeTypeNamedValueCollection()
        {
            var dcf = new TestDatumConverterFactory();
            var datum = new Datum()
            {
                type = Datum.DatumType.R_OBJECT,
                r_object =
                {
                    new Datum.AssocPair()
                    {
                        key = "key",
                        val = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = 100
                        }
                    }
                }
            };
            var nativeType = dcf.GetBestNativeTypeForDatum(datum);
            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(Dictionary<string, object>));
        }

        [Test]
        public void NativeTypeBool()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum() { type = Datum.DatumType.R_BOOL, r_bool = false });
            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(bool));
        }

        [Test]
        public void NativeTypeStringArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum()
                {
                    type = Datum.DatumType.R_ARRAY,
                    r_array =
                    {
                        new Datum { type = Datum.DatumType.R_STR, r_str = "Woot" },
                        new Datum { type = Datum.DatumType.R_NULL },
                    }
                });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(string[]));
        }

        [Test]
        public void NativeTypeBoolArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum()
                {
                    type = Datum.DatumType.R_ARRAY,
                    r_array =
                    {
                        new Datum { type = Datum.DatumType.R_BOOL, r_bool = true },
                        new Datum { type = Datum.DatumType.R_BOOL, r_bool = false },
                    }
                });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(bool[]));
        }

        [Test]
        public void NativeTypeNullableBoolArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum()
                {
                    type = Datum.DatumType.R_ARRAY,
                    r_array =
                    {
                        new Datum { type = Datum.DatumType.R_BOOL, r_bool = true },
                        new Datum { type = Datum.DatumType.R_NULL },
                        new Datum { type = Datum.DatumType.R_BOOL, r_bool = true },
                    }
                });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(bool?[]));
        }

        [Test]
        public void NativeTypeIntArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
                r_array =
                {
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 5 },
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 6 },
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 7 },
                }
            });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(int[]));
        }

        [Test]
        public void NativeTypeNullableIntArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
                r_array =
                {
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 5 },
                    new Datum { type = Datum.DatumType.R_NULL },
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 7 },
                }
                });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(int?[]));
        }

        [Test]
        public void NativeTypeDoubleArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
                r_array =
                {
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 5.1 },
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 7.2 },
                }
                });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(double[]));
        }

        [Test]
        public void NativeTypeNullableDoubleArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
                r_array =
                {
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 5.1 },
                    new Datum { type = Datum.DatumType.R_NULL },
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 7.2 },
                }
                });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(double?[]));
        }

        [Test]
        public void NativeTypeDoubleAndIntArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
                r_array =
                {
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 5.1 },
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 7 },
                }
                });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(double[]));
        }

        [Test]
        public void NativeTypeNullableDoubleAndIntArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
                r_array =
                {
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 5.1 },
                    new Datum { type = Datum.DatumType.R_NULL },
                    new Datum { type = Datum.DatumType.R_NUM, r_num = 7 },
                }
            });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(double?[]));
        }

        [Test]
        public void NativeTypeDateTimeOffsetArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
                r_array =
                {
                    dateTimeOffsetDatum,
                    dateTimeOffsetDatum,
                    dateTimeOffsetDatum
                }
            });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(DateTimeOffset[]));
        }

        [Test]
        public void NativeTypeNullableDateTimeOffsetArray()
        {
            var dcf = new TestDatumConverterFactory();

            var nativeType = dcf.GetBestNativeTypeForDatum(
                new Datum()
            {
                type = Datum.DatumType.R_ARRAY,
                r_array =
                {
                    dateTimeOffsetDatum,
                    new Datum { type = Datum.DatumType.R_NULL },
                    dateTimeOffsetDatum
                }
            });

            nativeType.Should().NotBeNull();
            nativeType.Should().Be(typeof(DateTimeOffset?[]));
        }

        [Test]
        public void NativeTypeHeterogeneousTypeError()
        {
            var dcf = new TestDatumConverterFactory();

            Action test = () => 
                dcf.GetBestNativeTypeForDatum(
                    new Datum()
                {
                    type = Datum.DatumType.R_ARRAY,
                    r_array =
                    {
                        new Datum { type = Datum.DatumType.R_STR, r_str = "5" },
                        new Datum { type = Datum.DatumType.R_NUM, r_num = 5 },
                    }
                });

            test.ShouldThrow<RethinkDbException>().WithMessage("Heterogeneous arrays are not currently supported as their types are indistinguishable");
        }
    }
}
