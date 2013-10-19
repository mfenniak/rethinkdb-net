using NUnit.Framework;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class EnumDatumConverterFactoryTests
    {
        enum TestEnumInt32 : int
        {
            Value1 = 1,
            Value1000 = 1000,
        }

        [Test]
        public void Int32ConvertObject()
        {
            var datumConverter = EnumDatumConverterFactory.Instance.Get<TestEnumInt32>();
            var datum = datumConverter.ConvertObject(TestEnumInt32.Value1);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(datum.r_num, Is.EqualTo(1));
        }

        [Test]
        public void Int32ConvertDatum()
        {
            var datumConverter = EnumDatumConverterFactory.Instance.Get<TestEnumInt32>();
            var datum = new Datum()
            {
                type = Datum.DatumType.R_NUM,
                r_num = 1,
            };
            var value = datumConverter.ConvertDatum(datum);
            Assert.That(value, Is.EqualTo(TestEnumInt32.Value1));
        }

        [Test]
        public void Int32ConvertDatumInvalidEnum()
        {
            var datumConverter = EnumDatumConverterFactory.Instance.Get<TestEnumInt32>();
            var datum = new Datum()
            {
                type = Datum.DatumType.R_NUM,
                r_num = 500,
            };
            var value = datumConverter.ConvertDatum(datum);
            Assert.That(value, Is.EqualTo((TestEnumInt32)500));
        }

        enum TestEnumUInt64 : ulong
        {
            Value1 = (1L << 34),
        }

        [Test]
        public void UInt64ConvertObject()
        {
            var datumConverter = EnumDatumConverterFactory.Instance.Get<TestEnumUInt64>();
            var datum = datumConverter.ConvertObject(TestEnumUInt64.Value1);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(datum.r_num, Is.EqualTo(1L << 34));
        }

        [Test]
        public void UInt64ConvertDatum()
        {
            var datumConverter = EnumDatumConverterFactory.Instance.Get<TestEnumUInt64>();
            var datum = new Datum()
            {
                type = Datum.DatumType.R_NUM,
                r_num = 1L << 34,
            };
            var value = datumConverter.ConvertDatum(datum);
            Assert.That(value, Is.EqualTo(TestEnumUInt64.Value1));
        }

        enum TestFlagsEnum
        {
            Flag1 = (1 << 0),
            Flag2 = (1 << 1),
            Flag3 = (1 << 2),
            Flag4 = (1 << 3),
            Flag5 = (1 << 4), 
        }

        [Test]
        public void FlagsConvertObject()
        {
            var datumConverter = EnumDatumConverterFactory.Instance.Get<TestFlagsEnum>();
            var datum = datumConverter.ConvertObject(TestFlagsEnum.Flag2 | TestFlagsEnum.Flag4);
            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_NUM));
            Assert.That(datum.r_num, Is.EqualTo(10));
        }

        [Test]
        public void FlagsConvertDatum()
        {
            var datumConverter = EnumDatumConverterFactory.Instance.Get<TestFlagsEnum>();
            var datum = new Datum()
            {
                type = Datum.DatumType.R_NUM,
                r_num = 10,
            };
            var value = datumConverter.ConvertDatum(datum);
            Assert.That(value, Is.EqualTo(TestFlagsEnum.Flag2 | TestFlagsEnum.Flag4));
        }
    }
}
