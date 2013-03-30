using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.QueryTerm;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RethinkDb.Test
{
    [TestFixture]
    public class MultiObjectTests : TestBase
    {
        private TableQuery<TestObject> testTable;

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
            connection.RunAsync(testTable.Insert(new TestObject[] {
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
            connection.RunAsync(testTable.Delete()).Wait();
        }

        [Test]
        public void Delete()
        {
            DoDelete().Wait();
        }

        private async Task DoDelete()
        {
            var resp = await connection.RunAsync(testTable.Delete());
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
            var enumerable = connection.RunAsync(testTable.Between("2", "4"));
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
            var enumerable = connection.RunAsync(testTable.Between(null, "4"));
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
            var resp = await connection.RunAsync(testTable.Between(null, "4").Delete());
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
            var resp = await connection.RunAsync(testTable.Update(o => new TestObject() { Name = "Hello!" }));
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
            var resp = await connection.RunAsync(testTable.Update(o => new TestObject() { Name = "Hello " + o.Id + "!" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            // "Replaced" seems weird here, rather than Updated, but that's what RethinkDB returns in the Data Explorer too...
            Assert.That(resp.Replaced, Is.EqualTo(7));
        }

        [Test]
        public void BetweenUpdate()
        {
            DoBetweenUpdate().Wait();
        }

        private async Task DoBetweenUpdate()
        {
            var resp = await connection.RunAsync(testTable.Between(null, "4").Update(o => new TestObject() { Name = "Hello " + o.Id + "!" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Replaced, Is.EqualTo(4));
        }

        [Test]
        public void Count()
        {
            DoCount().Wait();
        }

        private async Task DoCount()
        {
            var resp = await connection.RunAsync(testTable.Count());
            Assert.That(resp, Is.EqualTo(7));
        }

        [Test]
        public void BetweenCount()
        {
            DoBetweenCount().Wait();
        }

        private async Task DoBetweenCount()
        {
            var resp = await connection.RunAsync(testTable.Between(null, "4").Count());
            Assert.That(resp, Is.EqualTo(4));
        }

        private async Task DoFilterExpectedObjects(Expression<Func<TestObject, bool>> expr, params string[] expectedIds)
        {
            var enumerable = connection.RunAsync(testTable.Filter(expr));
            Assert.That(enumerable, Is.Not.Null);
            List<TestObject> objects = new List<TestObject>();
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                objects.Add(enumerable.Current);
                ++count;
            }
            Assert.That(count, Is.EqualTo(expectedIds.Length));
            Assert.That(objects, Has.Count.EqualTo(expectedIds.Length));
            foreach (var id in expectedIds)
                Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = id }));
        }

        [Test]
        public void FilterEqual()
        {
            DoFilterExpectedObjects(o => o.Name ==  "5", "5").Wait();
        }

        [Test]
        public void FilterLessThan()
        {
            DoFilterExpectedObjects(o => o.SomeNumber < 2, "1").Wait();
        }

        [Test]
        public void FilterLessThanEqual()
        {
            DoFilterExpectedObjects(o => o.SomeNumber <= 1, "1").Wait();
        }

        [Test]
        public void FilterGreaterThan()
        {
            DoFilterExpectedObjects(o => o.SomeNumber > 6, "7").Wait();
        }

        [Test]
        public void FilterGreaterThanEqual()
        {
            DoFilterExpectedObjects(o => o.SomeNumber >= 7, "7").Wait();
        }

        [Test]
        public void FilterAnd()
        {
            DoFilterExpectedObjects(o => o.SomeNumber == 7 && o.Name == "7", "7").Wait();
        }

        [Test]
        public void FilterOr()
        {
            DoFilterExpectedObjects(o => o.Name == "2" || o.Name == "4", "2", "4").Wait();
        }

        [Test]
        public void FilterNotEqual()
        {
            DoFilterExpectedObjects(o => o.Name != "3", new string[] { "1", "2", "4", "5", "6", "7" }).Wait();
        }

        [Test]
        public void FilterNot()
        {
            DoFilterExpectedObjects(o => !(o.Name == "3"), new string[] { "1", "2", "4", "5", "6", "7" }).Wait();
        }

        [Test]
        public void Map()
        {
            DoMap().Wait();
        }

        private async Task DoMap()
        {
            var enumerable = connection.RunAsync(testTable.Map(original => new AnotherTestObject() {
                FirstName = original.Name,
                LastName = original.Name + " (?)",
            }));

            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
            }
            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void MapToPrimitive()
        {
            DoMapToPrimitive().Wait();
        }

        private async Task DoMapToPrimitive()
        {
            var enumerable = connection.RunAsync(testTable.Map(original => original.SomeNumber));

            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
            }
            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void OrderByMultiField()
        {
            DoOrderByMultiField().Wait();
        }

        private async Task DoOrderByMultiField()
        {
            var enumerable = connection.RunAsync(testTable.OrderBy(
                o => o.Name,
                o => Query.Asc(o.Name),
                o => Query.Desc(o.Name)
            ));
            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
                Assert.That(enumerable.Current.Name, Is.EqualTo(count.ToString()));
            }
            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void OrderByAsc()
        {
            DoOrderByAsc().Wait();
        }

        private async Task DoOrderByAsc()
        {
            var enumerable = connection.RunAsync(testTable.OrderBy(o => Query.Asc(o.Name)));
            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
                Assert.That(enumerable.Current.Name, Is.EqualTo(count.ToString()));
            }
            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void OrderByDesc()
        {
            DoOrderByDesc().Wait();
        }

        private async Task DoOrderByDesc()
        {
            var enumerable = connection.RunAsync(testTable.OrderBy(o => Query.Desc(o.Name)));
            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
                Assert.That(enumerable.Current.Name, Is.EqualTo((8 - count).ToString()));
            }
            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void Reduce()
        {
            DoReduce().Wait();
        }

        private async Task DoReduce()
        {
            var resp = await connection.RunAsync(testTable.Reduce((acc, val) => new TestObject() { SomeNumber = acc.SomeNumber + val.SomeNumber }));
            Assert.That(resp.SomeNumber, Is.EqualTo(7 + 6 + 5 + 4 + 3 + 2 + 1 + 0));
        }

        [Test]
        public void ReduceToPrimitive()
        {
            DoReduceToPrimitive().Wait();
        }

        private async Task DoReduceToPrimitive()
        {
            var resp = await connection.RunAsync(testTable.Map(o => o.SomeNumber).Reduce((acc, val) => acc + val));
            Assert.That(resp, Is.EqualTo(7 + 6 + 5 + 4 + 3 + 2 + 1 + 0));
        }

        [Test]
        public void Skip()
        {
            DoSkip().Wait();
        }

        private async Task DoSkip()
        {
            var enumerable = connection.RunAsync(testTable.OrderBy(o => o.Name).Skip(6));
            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
                Assert.That(enumerable.Current.Name, Is.EqualTo("7"));
            }
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Limit()
        {
            DoLimit().Wait();
        }

        private async Task DoLimit()
        {
            var enumerable = connection.RunAsync(testTable.OrderBy(o => o.Name).Limit(1));
            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
                Assert.That(enumerable.Current.Name, Is.EqualTo("1"));
            }
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Slice()
        {
            DoSlice().Wait();
        }

        private async Task DoSlice()
        {
            var enumerable = connection.RunAsync(testTable.OrderBy(o => o.Name).Slice(1, 2));
            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
                Assert.That(enumerable.Current.SomeNumber, Is.EqualTo(count + 1));
            }
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void SliceStartOnly()
        {
            DoSliceStartOnly().Wait();
        }

        private async Task DoSliceStartOnly()
        {
            var enumerable = connection.RunAsync(testTable.OrderBy(o => o.Name).Slice(6));
            Assert.That(enumerable, Is.Not.Null);
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
                Assert.That(enumerable.Current.Name, Is.EqualTo("7"));
            }
            Assert.That(count, Is.EqualTo(1));
        }
    }
}
