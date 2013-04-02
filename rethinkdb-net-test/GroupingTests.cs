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
    public class GroupingTests : TestBase
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
                new TestObject() { Name = "1", SomeNumber = 1 },
                new TestObject() { Name = "1", SomeNumber = 1 },
                new TestObject() { Name = "2", SomeNumber = 2 },
                new TestObject() { Name = "2", SomeNumber = 200 },
                new TestObject() { Name = "2", SomeNumber = 2 },
                new TestObject() { Name = "3", SomeNumber = 3 },
                new TestObject() { Name = "3", SomeNumber = 3 },
                new TestObject() { Name = "4", SomeNumber = 4 },
                new TestObject() { Name = "5", SomeNumber = 5 },
                new TestObject() { Name = "6", SomeNumber = 6 },
                new TestObject() { Name = "6", SomeNumber = 6 },
                new TestObject() { Name = "7", SomeNumber = 7 },
            })).Wait();
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync(testTable.Delete()).Wait();
        }

        [Test]
        public void GroupedMapReduce()
        {
            var query = testTable.GroupedMapReduce(
                to => to.Name,  // group
                to => 1.0,        // map
                (leftCount, rightCount) => leftCount + rightCount // reduce
            );

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Item1;
                var reduceCount = record.Item2;

                switch (groupName)
                {
                    case "1":
                    case "3":
                    case "6":
                        Assert.That(reduceCount, Is.EqualTo(2));
                        break;
                    case "2":
                        Assert.That(reduceCount, Is.EqualTo(3));
                        break;
                    case "4":
                    case "5":
                    case "7":
                        Assert.That(reduceCount, Is.EqualTo(1));
                        break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void GroupByCount()
        {
            // Same query and results as GroupedMapReduce test
            var query = testTable.GroupBy(Query.Count(), to => to.Name);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Item1.Item1;
                var reduceCount = record.Item2;

                switch (groupName)
                {
                    case "1":
                    case "3":
                    case "6":
                        Assert.That(reduceCount, Is.EqualTo(2));
                        break;
                    case "2":
                        Assert.That(reduceCount, Is.EqualTo(3));
                        break;
                    case "4":
                    case "5":
                    case "7":
                        Assert.That(reduceCount, Is.EqualTo(1));
                        break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void GroupByTwoParams()
        {
            var query = testTable.GroupBy(Query.Count(), to => to.Name, to => to.SomeNumber);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Item1.Item1;
                var someNumber = record.Item1.Item2;
                var reduceCount = record.Item2;

                switch (groupName)
                {
                    case "1":
                    case "3":
                    case "6":
                        Assert.That(reduceCount, Is.EqualTo(2));
                        break;
                    case "2":
                        if (someNumber == 2)
                            Assert.That(reduceCount, Is.EqualTo(2));
                        else if (someNumber == 200)
                            Assert.That(reduceCount, Is.EqualTo(1));
                        break;
                    case "4":
                    case "5":
                    case "7":
                        Assert.That(reduceCount, Is.EqualTo(1));
                        break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(8));
        }
    }
}
