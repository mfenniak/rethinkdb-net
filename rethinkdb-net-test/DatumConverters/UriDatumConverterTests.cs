using System;
using NUnit.Framework;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class UriDatumConverterTests
    {
        private static readonly IDatumConverter<Uri> converter = UriDatumConverterFactory.Instance.Get<Uri>();

        [Test]
        public void ConvertDatumToObjectToDatum_Simple()
        {
            var datum1 = new RethinkDb.Spec.Datum(){
                type = RethinkDb.Spec.Datum.DatumType.R_STR,
                r_str = "http://www.example.com/"
            };
            var obj = converter.ConvertDatum(datum1);
            var datum2 = converter.ConvertObject(obj);

            Assert.That(datum2.type, Is.EqualTo(datum1.type));
            Assert.That(datum2.r_str, Is.EqualTo(datum1.r_str));
        }

        [Test]
        public void ConvertDatumToObjectToDatum_PathEncoding()
        {
            var datum1 = new RethinkDb.Spec.Datum(){
                type = RethinkDb.Spec.Datum.DatumType.R_STR,
                r_str = "http://www.example.com/dir1%2fdir1/dir2-dir2/file.txt"
            };
            var obj = converter.ConvertDatum(datum1);
            var datum2 = converter.ConvertObject(obj);

            Assert.That(datum2.type, Is.EqualTo(datum1.type));
            Assert.That(datum2.r_str, Is.EqualTo(datum1.r_str));
        }

        [Test]
        public void ConvertDatumToObjectToDatum_FileEncoding()
        {
            var datum1 = new RethinkDb.Spec.Datum(){
                type = RethinkDb.Spec.Datum.DatumType.R_STR,
                r_str = "http://www.example.com/dir1-dir1/dir2-dir2/file%2ffile.txt"
            };
            var obj = converter.ConvertDatum(datum1);
            var datum2 = converter.ConvertObject(obj);

            Assert.That(datum2.type, Is.EqualTo(datum1.type));
            Assert.That(datum2.r_str, Is.EqualTo(datum1.r_str));
        }

        [Test]
        public void ConvertDatumToObjectToDatum_QueryEncoding()
        {
            var datum1 = new RethinkDb.Spec.Datum(){
                type = RethinkDb.Spec.Datum.DatumType.R_STR,
                r_str = "http://www.example.com/service?data1%3ddata2=yes%3f%26true"
            };
            var obj = converter.ConvertDatum(datum1);
            var datum2 = converter.ConvertObject(obj);

            Assert.That(datum2.type, Is.EqualTo(datum1.type));
            Assert.That(datum2.r_str, Is.EqualTo(datum1.r_str));
        }

        [Test]
        public void ConvertDatumToObjectToDatum_HashEncoding()
        {
            var datum1 = new RethinkDb.Spec.Datum(){
                type = RethinkDb.Spec.Datum.DatumType.R_STR,
                r_str = "http://www.example.com/service#section%231"
            };
            var obj = converter.ConvertDatum(datum1);
            var datum2 = converter.ConvertObject(obj);

            Assert.That(datum2.type, Is.EqualTo(datum1.type));
            Assert.That(datum2.r_str, Is.EqualTo(datum1.r_str));
        }
    }
}

