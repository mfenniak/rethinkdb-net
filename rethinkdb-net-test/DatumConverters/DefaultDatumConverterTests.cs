using System;
using System.Linq;
using NUnit.Framework;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Test.DatumConverters
{
    // Tests about the default datum converter configured on Connection.
    [TestFixture]
    public class DefaultDatumConverterTests
    {
        // Ensure that byte[]'s are converted to RethinkDB's BINARY pseudotype, not as array objects
        [Test]
        public void BinaryPreferredOverArray()
        {
            var connection = new Connection();
            var datum = connection.QueryConverter.Get<byte[]>().ConvertObject(new byte[] { 0, 0, 0, 0 });

            Assert.That(datum.type, Is.EqualTo(Datum.DatumType.R_OBJECT));
            var keys = datum.r_object.ToDictionary(kvp => kvp.key, kvp => kvp.val);

            Assert.That(keys["$reql_type$"].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(keys["$reql_type$"].r_str, Is.EqualTo("BINARY"));

            Assert.That(keys["data"].type, Is.EqualTo(Datum.DatumType.R_STR));
            Assert.That(keys["data"].r_str, Is.EqualTo("AAAAAA=="));
        }
    }
}
