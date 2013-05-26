using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class DatabaseTests : TestBase
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.RunAsync(Query.DbCreate("test")).Wait();
        }

        [Test]
        public void TableCreateTest()
        {
            DoTableCreateTest().Wait();
        }

        private async Task DoTableCreateTest()
        {
            var testDb = Query.Db("test");

            var resp = await connection.RunAsync(testDb.TableCreate("table"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Created, Is.EqualTo(1));

            var tableList = await connection.RunAsync(testDb.TableList());
            Assert.That(tableList, Is.Not.Null);
            Assert.That(tableList, Contains.Item("table"));

            resp = await connection.RunAsync(testDb.TableDrop("table"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Dropped, Is.EqualTo(1));
        }
    }
}

