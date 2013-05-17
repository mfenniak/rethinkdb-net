using System;
using NUnit.Framework;

namespace RethinkDb.Test.DatumConverters
{
    [TestFixture]
    public class NullableGuidDatumConverterTests
    {
        [Test]
        public void ConvertDatum_ValidDatum_ReturnsGuid()
        {
            var guid = Guid.NewGuid();

            var result = GuidDatumConverterFactory.Instance.Get<Guid?>().ConvertDatum(new RethinkDb.Spec.Datum(){type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = guid.ToString()});

            Assert.AreEqual(guid.ToString(), result.ToString(), "should match");
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ConvertDatum_InvalidGuid_ThrowsException()
        {
            var guidString = "NonsenseStringThatDoesNotSerializeToGuid";

            GuidDatumConverterFactory.Instance.Get<Guid?>().ConvertDatum(new RethinkDb.Spec.Datum() {type = RethinkDb.Spec.Datum.DatumType.R_STR, r_str = guidString});
        }

        [Test]
        public void ConvertObject_ValidGuid_ReturnsDatum()
        {
            var guid = Guid.NewGuid();

            var result = GuidDatumConverterFactory.Instance.Get<Guid?>().ConvertObject(guid);

            Assert.AreEqual(guid.ToString(), result.r_str, "should match");
        }
    }
}

