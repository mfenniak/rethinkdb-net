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
        public void TableCreateListDrop()
        {
            DoTableCreateListDrop().Wait();
        }

        private async Task DoTableCreateListDrop()
        {
            var testDb = Query.Db("test");

            var resp = await connection.RunAsync(testDb.TableCreate("TableCreateListDrop"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Created, Is.EqualTo(1));

            var tableList = await connection.RunAsync(testDb.TableList());
            Assert.That(tableList, Is.Not.Null);
            Assert.That(tableList, Contains.Item("TableCreateListDrop"));

            resp = await connection.RunAsync(testDb.TableDrop("TableCreateListDrop"));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Dropped, Is.EqualTo(1));
        }

        [Test]
        public void TableCreateEmptyDataCenter()
        {
            connection.Run(Query.Db("test").TableCreate("TableCreateEmptyDataCenter", datacenter: ""));
        }

        [Test]
        public void TableCreateNullDataCenter()
        {
            connection.Run(Query.Db("test").TableCreate("TableCreateNullDataCenter", datacenter: null));
        }

        [Test]
        public void TableCreateEmptyPrimaryKey()
        {
            connection.Run(Query.Db("test").TableCreate("TableCreateEmptyPrimaryKey", primaryKey: ""));
        }

        [Test]
        public void TableCreateNullPrimaryKey()
        {
            connection.Run(Query.Db("test").TableCreate("TableCreateNullPrimaryKey", primaryKey: null));
        }
    }
}

