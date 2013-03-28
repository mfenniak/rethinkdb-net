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
            connection.Run(Query.DbCreate("test")).Wait();
            connection.Run(Query.Db("test").TableCreate("table1")).Wait();
            connection.Run(Query.Db("test").TableCreate("table2")).Wait();
        }

        [SetUp]
        public virtual void SetUp()
        {
            testTable = Query.Db("test").Table<TestObject>("table1");
            anotherTestTable = Query.Db("test").Table<AnotherTestObject>("table2");

            connection.Run(testTable.Insert(new TestObject[] {
                new TestObject() { Id = "1", Name = "1", SomeNumber = 1 },
                new TestObject() { Id = "2", Name = "2", SomeNumber = 2 },
                new TestObject() { Id = "3", Name = "3", SomeNumber = 3 },
            })).Wait();

            connection.Run(anotherTestTable.Insert(new AnotherTestObject[] {
                new AnotherTestObject() { Id = "1", FirstName = "1", LastName = "1" },
                new AnotherTestObject() { Id = "2", FirstName = "2", LastName = "2" },
                new AnotherTestObject() { Id = "3", FirstName = "3", LastName = "3" },
            })).Wait();
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.Run(testTable.Delete()).Wait();
            connection.Run(anotherTestTable.Delete()).Wait();
        }

        [Test]
        public void Join()
        {
            DoJoin().Wait();
        }

        private async Task DoJoin()
        {
            var enumerable = connection.Run(
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
    }
}
