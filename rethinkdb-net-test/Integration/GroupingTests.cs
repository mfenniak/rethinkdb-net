using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.QueryTerm;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RethinkDb.Test.Integration
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
            var query = testTable.GroupBy(Query.Count(), to => new { name = to.Name });

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Item1.name;
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
            var query = testTable.GroupBy(Query.Count(), to => new { name = to.Name, number = to.SomeNumber });

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Item1.name;
                var someNumber = record.Item1.number;
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

        [Test]
        public void GroupBySum()
        {
            var query = testTable.GroupBy(Query.Sum<TestObject>(to => to.SomeNumber), to => new { name = to.Name });

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Item1.name;
                var reduceSum = record.Item2;

                switch (groupName)
                {
                    case "1":
                        Assert.That(reduceSum, Is.EqualTo(2));
                        break;
                    case "2":
                        Assert.That(reduceSum, Is.EqualTo(204));
                        break;
                    case "3":
                        Assert.That(reduceSum, Is.EqualTo(6));
                        break;
                    case "4":
                        Assert.That(reduceSum, Is.EqualTo(4));
                        break;
                    case "5":
                        Assert.That(reduceSum, Is.EqualTo(5));
                        break;
                    case "6":
                        Assert.That(reduceSum, Is.EqualTo(12));
                        break;
                    case "7":
                        Assert.That(reduceSum, Is.EqualTo(7));
                        break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void GroupByAvg()
        {
            var query = testTable.GroupBy(Query.Avg<TestObject>(to => to.SomeNumber), to => new { name = to.Name });

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Item1.name;
                var reduceSum = record.Item2;

                switch (groupName)
                {
                    case "1":
                        Assert.That(reduceSum, Is.EqualTo(1));
                        break;
                    case "2":
                        Assert.That(reduceSum, Is.EqualTo(68));
                        break;
                    case "3":
                        Assert.That(reduceSum, Is.EqualTo(3));
                        break;
                    case "4":
                        Assert.That(reduceSum, Is.EqualTo(4));
                        break;
                    case "5":
                        Assert.That(reduceSum, Is.EqualTo(5));
                        break;
                    case "6":
                        Assert.That(reduceSum, Is.EqualTo(6));
                        break;
                    case "7":
                        Assert.That(reduceSum, Is.EqualTo(7));
                        break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(7));
        }
    }
}
