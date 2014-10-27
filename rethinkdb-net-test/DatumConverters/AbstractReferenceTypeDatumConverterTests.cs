using System;
using NUnit.Framework;
using RethinkDb.Spec;
using RethinkDb.DatumConverters;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class AbstractReferenceTypeDatumConverterTests
    {
        public class TestDatumConverter : AbstractReferenceTypeDatumConverter<Uri>
        {
            public override Uri ConvertDatum(Datum datum)
            {
                if (datum.type == Datum.DatumType.R_NULL)
                    return null;
                else
                    return new Uri("http://www.google.ca/");
            }

            public override Datum ConvertObject(Uri value)
            {
                if (value == null)
                    return new Datum() { type = Datum.DatumType.R_NULL };
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
            Assert.That(retval, Is.EqualTo(new Uri("http://www.google.ca/")));
        }

        [Test]
        public void NonGenericConvertDatumNull()
        {
            var dc = (IDatumConverter)new TestDatumConverter();
            var retval = dc.ConvertDatum(new Datum() { type = Datum.DatumType.R_NULL });
            Assert.That(retval, Is.Null);
        }

        [Test]
        public void NonGenericConvertObject()
        {
            var dc = (IDatumConverter)new TestDatumConverter();
            var retval = dc.ConvertObject(new Uri("http://www.google.ca/"));
            Assert.That(retval, Is.Not.Null);
            Assert.That(retval.type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(retval.r_num, Is.EqualTo(100));
        }

        [Test]
        public void NonGenericConvertObjectNull()
        {
            var dc = (IDatumConverter)new TestDatumConverter();
            var retval = dc.ConvertObject(null);
            Assert.That(retval, Is.Not.Null);
            Assert.That(retval.type, Is.EqualTo(Datum.DatumType.R_NULL));
        }
    }
}

