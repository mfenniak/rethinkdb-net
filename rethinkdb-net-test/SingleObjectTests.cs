using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.QueryTerm;
using System.Linq.Expressions;

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
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
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
                },
                SomeNumber = 1234
            };
            var resp = await connection.RunAsync(testTable.Insert(insertedObject));
            insertedObject.Id = resp.GeneratedKeys[0];
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync(testTable.Delete()).Wait();
        }

        [Test]
        public void GetQueryNull()
        {
            DoGetQueryNull().Wait();
        }

        private async Task DoGetQueryNull()
        {
            var obj = await connection.RunAsync(testTable.Get(insertedObject.Id));
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
            var resp = await connection.RunAsync(testTable.Get(insertedObject.Id).Replace(new TestObject() { Id = insertedObject.Id, Name = "Jack Black" }));
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
            var resp = await connection.RunAsync(testTable.Get(insertedObject.Id).Delete());
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Deleted, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }

        [Test]
        public void GetUpdateNumericAdd()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber + 1 }, 1235).Wait();
        }

        [Test]
        public void GetUpdateNumericSub()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber - 1 }, 1233).Wait();
        }

        [Test]
        public void GetUpdateNumericDiv()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber / 2 }, 617).Wait();
        }

        [Test]
        public void GetUpdateNumericMul()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber * 2 }, 2468).Wait();
        }

        [Test]
        public void GetUpdateNumericMod()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber % 600 }, 34).Wait();
        }

        private async Task DoGetUpdateNumeric(Expression<Func<TestObject, TestObject>> expr, double expected)
        {
            var resp = await connection.RunAsync(testTable.Get(insertedObject.Id).Update(expr));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            // "Replaced" seems weird here, rather than Updated, but that's what RethinkDB returns in the Data Explorer too...
            Assert.That(resp.Replaced, Is.EqualTo(1));

            var obj = await connection.RunAsync(testTable.Get(insertedObject.Id));
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.SomeNumber, Is.EqualTo(expected));
        }

        [Test]
        public void Reduce()
        {
            DoReduce().Wait();
        }

        private async Task DoReduce()
        {
            var resp = await connection.RunAsync(testTable.Reduce((acc, val) => new TestObject() { SomeNumber = acc.SomeNumber + val.SomeNumber }));
            Assert.That(resp.SomeNumber, Is.EqualTo(1234));
        }

        [Test]
        public void ReduceToPrimitive()
        {
            DoReduceToPrimitive().Wait();
        }

        private async Task DoReduceToPrimitive()
        {
            var resp = await connection.RunAsync(testTable.Map(o => o.SomeNumber).Reduce((acc, val) => acc + val));
            Assert.That(resp, Is.EqualTo(1234));
        }
    }
}

