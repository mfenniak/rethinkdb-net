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
    public class MultiTableTests : TestBase
    {
        private TableQuery<TestObject> testTable;
        private TableQuery<AnotherTestObject> anotherTestTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table1")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table2")).Wait();
        }

        [SetUp]
        public virtual void SetUp()
        {
            testTable = Query.Db("test").Table<TestObject>("table1");
            anotherTestTable = Query.Db("test").Table<AnotherTestObject>("table2");

            connection.RunAsync(testTable.Insert(new TestObject[] {
                new TestObject() { Id = "1", Name = "1", SomeNumber = 1, Children = new TestObject[1] },
                new TestObject() { Id = "2", Name = "2", SomeNumber = 2, Children = new TestObject[2] },
                new TestObject() { Id = "3", Name = "3", SomeNumber = 3, Children = new TestObject[3] },
                new TestObject() { Id = "4", Name = "4", SomeNumber = 4, Children = new TestObject[4] },
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
            DoEqJoin().Wait();
        }

        private async Task DoEqJoin()
        {
            var enumerable = connection.RunAsync(
                testTable.EqJoin(
                    testObject => testObject.Name,
                    anotherTestTable
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
            }
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));
        }
    }
}
