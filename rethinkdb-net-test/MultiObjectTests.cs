using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.QueryTerm;
using System.Collections.Generic;

namespace RethinkDb.Test
{
    [TestFixture]
    public class MultiObjectTests : TestBase
    {
        private TableQuery<TestObject> testTable;

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
            connection.Run(testTable.Insert(new TestObject[] {
                new TestObject() { Id = "1", Name = "1", SomeNumber = 1 },
                new TestObject() { Id = "2", Name = "2", SomeNumber = 2 },
                new TestObject() { Id = "3", Name = "3", SomeNumber = 3 },
                new TestObject() { Id = "4", Name = "4", SomeNumber = 4 },
                new TestObject() { Id = "5", Name = "5", SomeNumber = 5 },
                new TestObject() { Id = "6", Name = "6", SomeNumber = 6 },
                new TestObject() { Id = "7", Name = "7", SomeNumber = 7 },
            })).Wait();
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.Run(testTable.Delete()).Wait();
        }

        [Test]
        public void Delete()
        {
            DoDelete().Wait();
        }

        private async Task DoDelete()
        {
            var resp = await connection.Run(testTable.Delete());
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Deleted, Is.EqualTo(7));
        }

        [Test]
        public void Between()
        {
            DoBetween().Wait();
        }

        private async Task DoBetween()
        {
            var enumerable = connection.Run(testTable.Between("2", "4"));
            List<TestObject> objects = new List<TestObject>();
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                objects.Add(enumerable.Current);
                ++count;
            }
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));

            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "2" }));
            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "3" }));
            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "4" }));
        }

        [Test]
        public void BetweenNull()
        {
            DoBetweenNull().Wait();
        }

        private async Task DoBetweenNull()
        {
            var enumerable = connection.Run(testTable.Between(null, "4"));
            List<TestObject> objects = new List<TestObject>();
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                objects.Add(enumerable.Current);
                ++count;
            }
            Assert.That(count, Is.EqualTo(4));
            Assert.That(objects, Has.Count.EqualTo(4));

            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "1" }));
            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "2" }));
            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "3" }));
            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "4" }));
        }
        
        [Test]
        public void BetweenDelete()
        {
            DoBetweenDelete().Wait();
        }

        private async Task DoBetweenDelete()
        {
            var resp = await connection.Run(testTable.Between(null, "4").Delete());
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Deleted, Is.EqualTo(4));
        }

        [Test]
        public void SimpleUpdate()
        {
            DoSimpleUpdate().Wait();
        }

        private async Task DoSimpleUpdate()
        {
            var resp = await connection.Run(testTable.Update(o => new TestObject() { Name = "Hello!" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            // "Replaced" seems weird here, rather than Updated, but that's what RethinkDB returns in the Data Explorer too...
            Assert.That(resp.Replaced, Is.EqualTo(7));
        }

        [Test]
        public void RecordReferencingUpdate()
        {
            DoRecordReferencingUpdate().Wait();
        }

        private async Task DoRecordReferencingUpdate()
        {
            var resp = await connection.Run(testTable.Update(o => new TestObject() { Name = "Hello " + o.Id + "!" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            // "Replaced" seems weird here, rather than Updated, but that's what RethinkDB returns in the Data Explorer too...
            Assert.That(resp.Replaced, Is.EqualTo(7));
        }

        [Test]
        public void UpdateWithNumericalOperators()
        {
            DoUpdateWithNumericalOperators().Wait();
        }

        private async Task DoUpdateWithNumericalOperators()
        {
            var resp = await connection.Run(testTable.Update(o => new TestObject() { SomeNumber = (((o.SomeNumber + 1 - 1) * 2) / 2) % 1 }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            // "Replaced" seems weird here, rather than Updated, but that's what RethinkDB returns in the Data Explorer too...
            Assert.That(resp.Replaced, Is.EqualTo(7));

            var enumerable = connection.Run(testTable);
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;

                var obj = enumerable.Current;
                var originalNum = Double.Parse(obj.Id);
                Assert.That(obj.SomeNumber, Is.EqualTo((((originalNum + 1 - 1) * 2) / 2) % 1));
            }
        }
    }
}

