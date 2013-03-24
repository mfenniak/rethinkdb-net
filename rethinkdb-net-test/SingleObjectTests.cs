using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.QueryTerm;

namespace RethinkDb.Test
{
    [TestFixture]
    public class SingleObjectTests : TestBase
    {
        private TableQuery<TestObject> testTable;
        private TestObject insertedObject;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.Run(Query.DbCreate("test")).Wait();
            connection.Run(Query.Db("test").TableCreate("table")).Wait();
        }

        [SetUp]
        public virtual void SetUp()
        {
            testTable = Query.Db("test").Table<TestObject>("table");
            DoInsert().Wait();
        }

        private async Task DoInsert()
        {
            insertedObject = new TestObject()
            {
                Name = "Jim Brown",
                Children = new TestObject[] {
                    new TestObject() { Name = "Scan" }
                }
            };
            var resp = await connection.Run(testTable.Insert(insertedObject));
            insertedObject.Id = resp.GeneratedKeys[0];
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.Run(testTable.Delete()).Wait();
        }

        [Test]
        public void GetQueryNull()
        {
            DoGetQueryNull().Wait();
        }

        private async Task DoGetQueryNull()
        {
            var obj = await connection.Run(testTable.Get(insertedObject.Id));
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.Id, Is.EqualTo(insertedObject.Id));
        }

        [Test]
        public void Replace()
        {
            DoReplace().Wait();
        }

        private async Task DoReplace()
        {
            var resp = await connection.Run(testTable.Get(insertedObject.Id).Replace(new TestObject() { Id = insertedObject.Id, Name = "Jack Black" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Replaced, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }

        [Test]
        public void Delete()
        {
            DoDelete().Wait();
        }

        private async Task DoDelete()
        {
            var resp = await connection.Run(testTable.Get(insertedObject.Id).Delete());
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Deleted, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }
    }
}

