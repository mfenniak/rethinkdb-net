using System;
using NUnit.Framework;
using RethinkDb.Spec;

namespace RethinkDb.Test
{
    [TestFixture]
    public class AbstractValueTypeDatumConverterTests
    {
        public class TestDatumConverter : AbstractValueTypeDatumConverter<int>
        {
            public override int ConvertDatum(Datum datum)
            {
                return 100;
            }

            public override Datum ConvertObject(int value)
            {
                return new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = 100,
                };
            }
        }

        [Test]
        public void NonGenericConvertDatum()
        {
            var dc = (IDatumConverter)new TestDatumConverter();
            var retval = dc.ConvertDatum(new Datum() { type = Datum.DatumType.R_NUM });
            Assert.That(retval, Is.Not.Null);
            Assert.That(retval, Is.EqualTo(100));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void NonGenericConvertDatumNull()
        {
            var dc = (IDatumConverter)new TestDatumConverter();
            dc.ConvertDatum(new Datum() { type = Datum.DatumType.R_NULL });
        }

        [Test]
        public void NonGenericConvertObject()
        {
            var dc = (IDatumConverter)new TestDatumConverter();
            var retval = dc.ConvertObject(5);
            Assert.That(retval, Is.Not.Null);
            Assert.That(retval.type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(retval.r_num, Is.EqualTo(100));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void NonGenericConvertObjectNull()
        {
            var dc = (IDatumConverter)new TestDatumConverter();
            dc.ConvertObject(null);
        }
    }
}

