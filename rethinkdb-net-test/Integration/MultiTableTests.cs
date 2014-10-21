using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class MultiTableTests : TestBase
    {
        private ITableQuery<TestObject> testTable;
        private ITableQuery<AnotherTestObject> anotherTestTable;
        private IIndex<AnotherTestObject, string> firstNameIndex;
        private IMultiIndex<TestObject, string> tagsIndex;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table1")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table2")).Wait();

            testTable = Query.Db("test").Table<TestObject>("table1");
            anotherTestTable = Query.Db("test").Table<AnotherTestObject>("table2");

            firstNameIndex = anotherTestTable.IndexDefine("index1", o => o.FirstName);
            connection.Run(firstNameIndex.IndexCreate());

            tagsIndex = testTable.IndexDefineMulti("indexTags", o => o.Tags);
            connection.Run(tagsIndex.IndexCreate());
        }

        [SetUp]
        public virtual void SetUp()
        {
            connection.RunAsync(testTable.Insert(new TestObject[] {
                new TestObject() { Id = "1", Name = "1", SomeNumber = 1, Tags = new[] { "1", "5" }, Children = new TestObject[1], ChildrenList = new List<TestObject> { null }, ChildrenIList = new List<TestObject> { null } },
                new TestObject() { Id = "2", Name = "2", SomeNumber = 2, Tags = new[] { "2", "6" }, Children = new TestObject[2], ChildrenList = new List<TestObject> { null, null }, ChildrenIList = new List<TestObject> { null, null } },
                new TestObject() { Id = "3", Name = "3", SomeNumber = 3, Tags = new[] { "3", "7" }, Children = new TestObject[3], ChildrenList = new List<TestObject> { null, null, null }, ChildrenIList = new List<TestObject> { null, null, null } },
                new TestObject() { Id = "4", Name = "4", SomeNumber = 4, Tags = new[] { "4", "8" }, Children = new TestObject[4], ChildrenList = new List<TestObject> { null, null, null, null }, ChildrenIList = new List<TestObject> { null, null, null, null } },
            })).Wait();

            connection.RunAsync(anotherTestTable.Insert(new AnotherTestObject[] {
                new AnotherTestObject() { Id = "1", FirstName = "1", LastName = "1" },
                new AnotherTestObject() { Id = "2", FirstName = "2", LastName = "2" },
                new AnotherTestObject() { Id = "3", FirstName = "3", LastName = "3" },
            })).Wait();
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync(testTable.Delete()).Wait();
            connection.RunAsync(anotherTestTable.Delete()).Wait();
        }

        [Test]
        public void InnerJoin()
        {
            DoInnerJoin().Wait();
        }

        private async Task DoInnerJoin()
        {
            var enumerable = connection.RunAsync(
                testTable.InnerJoin(
                    anotherTestTable,
                    (testObject, anotherTestObject) => testObject.Name == anotherTestObject.FirstName
                )
            );
            Assert.That(enumerable, Is.Not.Null);

            var objects = new List<Tuple<TestObject, AnotherTestObject>>();
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                objects.Add(enumerable.Current);
                ++count;

                var tup = enumerable.Current;
                Assert.That(tup.Item1, Is.Not.Null);
                Assert.That(tup.Item2, Is.Not.Null);
                Assert.That(tup.Item1.Name, Is.EqualTo(tup.Item2.FirstName));
            }
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));
        }

        [Test]
        public void OuterJoin()
        {
            DoOuterJoin().Wait();
        }

        private async Task DoOuterJoin()
        {
            var enumerable = connection.RunAsync(
                testTable.OuterJoin(
                    anotherTestTable,
                    (testObject, anotherTestObject) => testObject.Name == anotherTestObject.FirstName
                )
            );
            Assert.That(enumerable, Is.Not.Null);

            var objects = new List<Tuple<TestObject, AnotherTestObject>>();
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                objects.Add(enumerable.Current);
                ++count;

                var tup = enumerable.Current;
                Assert.That(tup.Item1, Is.Not.Null);

                if (tup.Item1.Id == "4")
                {
                    Assert.That(tup.Item2, Is.Null);
                }
                else
                {
                    Assert.That(tup.Item2, Is.Not.Null);
                    Assert.That(tup.Item1.Name, Is.EqualTo(tup.Item2.FirstName));
                }
            }
            Assert.That(count, Is.EqualTo(4));
            Assert.That(objects, Has.Count.EqualTo(4));
        }

        [Test]
        public void EqJoin()
        {
            DoEqJoin(
                testTable.EqJoin(
                    testObject => testObject.Name,
                    anotherTestTable
                )
            ).Wait();
        }

        [Test]
        public void EqJoinNullIndex()
        {
            DoEqJoin(
                testTable.EqJoin(
                    testObject => testObject.Name,
                    anotherTestTable,
                    indexName: null
                )
            ).Wait();
        }

        [Test]
        public void EqJoinEmptyIndex()
        {
            DoEqJoin(
                testTable.EqJoin(
                    testObject => testObject.Name,
                    anotherTestTable,
                    indexName: ""
                )
            ).Wait();
        }

        private async Task DoEqJoin(ISequenceQuery<Tuple<TestObject, AnotherTestObject>> query)
        {
            var enumerable = connection.RunAsync(query);
            Assert.That(enumerable, Is.Not.Null);

            var objects = new List<Tuple<TestObject, AnotherTestObject>>();
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                objects.Add(enumerable.Current);
                ++count;

                var tup = enumerable.Current;
                Assert.That(tup.Item1, Is.Not.Null);

                Assert.That(tup.Item2, Is.Not.Null);
                Assert.That(tup.Item1.Name, Is.EqualTo(tup.Item2.FirstName));
            }
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));
        }

        [Test]
        public void EqJoinIndex()
        {
            var enumerable = connection.Run(
                testTable.EqJoin(
                    testObject => testObject.Name,
                    anotherTestTable,
                    "index1"
                )
            );
            Assert.That(enumerable, Is.Not.Null);

            var objects = new List<Tuple<TestObject, AnotherTestObject>>();
            var count = 0;
            foreach (var tup in enumerable)
            {
                objects.Add(tup);
                ++count;

                Assert.That(tup.Item1, Is.Not.Null);

                Assert.That(tup.Item2, Is.Not.Null);
                Assert.That(tup.Item1.Name, Is.EqualTo(tup.Item2.FirstName));
            }
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));
        }

        [Test]
        public void EqJoinIndexObject()
        {
            var enumerable = connection.Run(
                testTable.EqJoin(
                    testObject => testObject.Name,
                    anotherTestTable,
                    firstNameIndex
                )
            );
            Assert.That(enumerable, Is.Not.Null);

            var objects = new List<Tuple<TestObject, AnotherTestObject>>();
            var count = 0;
            foreach (var tup in enumerable)
            {
                objects.Add(tup);
                ++count;

                Assert.That(tup.Item1, Is.Not.Null);

                Assert.That(tup.Item2, Is.Not.Null);
                Assert.That(tup.Item1.Name, Is.EqualTo(tup.Item2.FirstName));
            }
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));
        }

        [Test]
        public void EqJoinMultiIndex()
        {
            var enumerable = connection.Run(
                anotherTestTable.EqJoin(
                    anotherTestObject => anotherTestObject.FirstName,
                    testTable,
                    tagsIndex
                )
            );
            Assert.That(enumerable, Is.Not.Null);

            var objects = new List<Tuple<AnotherTestObject, TestObject>>();
            var count = 0;
            foreach (var tup in enumerable)
            {
                objects.Add(tup);
                ++count;

                Assert.That(tup.Item1, Is.Not.Null);
                Assert.That(tup.Item2, Is.Not.Null);
                tup.Item2.Tags.Should().Contain(tup.Item1.FirstName);
            }
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));
        }

        [Test]
        public void Zip()
        {
            DoZip().Wait();
        }

        private async Task DoZip()
        {
            var enumerable = connection.RunAsync(
                testTable.EqJoin(
                    testObject => testObject.Name,
                    anotherTestTable
                )
                .Zip<TestObject, AnotherTestObject, ZipTestObject>()
            );
            Assert.That(enumerable, Is.Not.Null);

            var objects = new List<ZipTestObject>();
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                objects.Add(enumerable.Current);
                ++count;

                var obj = enumerable.Current;
                Assert.That(obj.Name, Is.EqualTo(obj.Id));
                Assert.That(obj.FirstName, Is.EqualTo(obj.Id));
                Assert.That(obj.LastName, Is.EqualTo(obj.Id));
                Assert.That(obj.Children.Length, Is.EqualTo(obj.SomeNumber));
                Assert.That(obj.ChildrenList.Count, Is.EqualTo(obj.SomeNumber));
                Assert.That(obj.ChildrenIList.Count, Is.EqualTo(obj.SomeNumber));
            }
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));
        }
    }
}
