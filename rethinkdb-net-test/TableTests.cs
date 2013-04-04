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
    public class TableTests : TestBase
    {
        private TableQuery<TestObject> testTable;
        private TableQuery<TestObject2> testTable2;

        [SetUp]
        public virtual void SetUp()
        {
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
            testTable = Query.Db("test").Table<TestObject>("table");
            testTable2 = Query.Db("test").Table<TestObject2>("table");
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync(Query.DbDrop("test")).Wait();
        }

        [Test]
        public void GetQueryNull()
        {
            DoGetQueryNull().Wait();
        }

        private async Task DoGetQueryNull()
        {
            var obj = await connection.RunAsync(testTable.Get("58379951-6208-46cc-a194-03da8ee1e13c"));
            Assert.That(obj, Is.Null);
        }

        [Test]
        public void EmptyEnumerator()
        {
            DoEmptyEnumerator().Wait();
        }

        private async Task DoEmptyEnumerator()
        {
            var enumerable = connection.RunAsync(testTable);
            int count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
            }
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void Insert()
        {
            DoInsert().Wait();
        }

        private async Task DoInsert()
        {
            var obj = new TestObject()
            {
                Name = "Jim Brown",
                Children = new TestObject[] {
                    new TestObject() { Name = "Scan" }
                }
            };
            var resp = await connection.RunAsync(testTable.Insert(obj));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Inserted, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Not.Null);
            Assert.That(resp.GeneratedKeys, Has.Length.EqualTo(1));
        }

        [Test]
        public void MultiInsert()
        {
            DoMultiInsert().Wait();
        }

        private async Task DoMultiInsert()
        {
            var resp = await connection.RunAsync(testTable.Insert(new TestObject[] {
                new TestObject() { Name = "1" },
                new TestObject() { Name = "2" },
                new TestObject() { Name = "3" },
                new TestObject() { Name = "4" },
                new TestObject() { Name = "5" },
                new TestObject() { Name = "6" },
                new TestObject() { Name = "7" },
            }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Inserted, Is.EqualTo(7));
            Assert.That(resp.GeneratedKeys, Is.Not.Null);
            Assert.That(resp.GeneratedKeys, Has.Length.EqualTo(7));
        }

        [Test]
        public void MultiInsertWithIds()
        {
            DoMultiInsertWithIds().Wait();
        }

        private async Task DoMultiInsertWithIds()
        {
            var resp = await connection.RunAsync(testTable.Insert(new TestObject[] {
                new TestObject() { Id = "1", Name = "1" },
                new TestObject() { Id = "2", Name = "2" },
                new TestObject() { Id = "3", Name = "3" },
                new TestObject() { Id = "4", Name = "4" },
                new TestObject() { Id = "5", Name = "5" },
                new TestObject() { Id = "6", Name = "6" },
                new TestObject() { Id = "7", Name = "7" },
            }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Inserted, Is.EqualTo(7));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }

        [Test]
        public void Reduce()
        {
            DoReduce().Wait();
        }

        private async Task DoReduce()
        {
            var resp = await connection.RunAsync(testTable.Reduce((acc, val) => new TestObject() { SomeNumber = acc.SomeNumber + val.SomeNumber }, new TestObject() { SomeNumber = -1 }));
            Assert.That(resp.SomeNumber, Is.EqualTo(-1));
        }

        [Test]
        public void ReduceToPrimitive()
        {
            DoReduceToPrimitive().Wait();
        }

        private async Task DoReduceToPrimitive()
        {
            var resp = await connection.RunAsync(testTable.Map(o => o.SomeNumber).Reduce((acc, val) => acc + val, -1.0));
            Assert.That(resp, Is.EqualTo(-1.0));
        }

        [Test]
        public void NumericIdInsert()
        {
            var resp = connection.Run(testTable2.Insert(new TestObject2() { Id = 1, Name = "Woot" }));
            Assert.That(resp.Inserted, Is.EqualTo(1));
            resp = connection.Run(testTable2.Insert(new TestObject2() { Id = 1, Name = "Woot" }));
            Assert.That(resp.Errors, Is.EqualTo(1));
            resp = connection.Run(testTable2.Insert(new TestObject2() { Id = 1, Name = "Woot" }));
            Assert.That(resp.Errors, Is.EqualTo(1));
        }

        [Test]
        public void NumericIdGet()
        {
            var resp = connection.Run(testTable2.Insert(new TestObject2() { Id = 1, Name = "Woot" }));
            Assert.That(resp.Inserted, Is.EqualTo(1));

            var to = connection.Run(testTable2.Get(1));
            Assert.That(to, Is.Not.Null);
            Assert.That(to.Id, Is.EqualTo(1));
        }
    }
}

