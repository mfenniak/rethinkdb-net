using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace RethinkDb.Test
{
    [TestFixture]
    public class BlankTests : TestBase
    {
        [Test]
        public void DbCreateTest()
        {
            DoDbCreateTest().Wait();
        }

        private async Task DoDbCreateTest()
        {
            var resp = await connection.Run(Query.DbCreate("test"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Created, Is.EqualTo(1));

            var dbList = await connection.Run(Query.DbList());
            Assert.That(dbList, Is.Not.Null);
            Assert.That(dbList, Contains.Item("test"));

            resp = await connection.Run(Query.DbDrop("test"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Dropped, Is.EqualTo(1));
        }
    }
}

