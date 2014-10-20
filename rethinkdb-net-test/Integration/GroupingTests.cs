using System;
using NUnit.Framework;
using RethinkDb;
using System.Linq;
using System.Collections.Generic;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class GroupingTests : TestBase
    {
        private ITableQuery<TestObject> testTable;

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
            connection.Run(testTable.Insert(new TestObject[]
            {
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
            }));
            connection.Run(testTable.IndexCreate("name", to => to.Name));
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.Run(testTable.IndexDrop("name"));
            connection.Run(testTable.Delete());
        }

        [Test]
        public void GroupByIndex()
        {
            var query = testTable.Group<TestObject, string>("name");

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Key;
                var objects = record.Value;

                switch (groupName)
                {
                    case "1":
                    case "3":
                    case "6":
                        Assert.That(objects.Count(), Is.EqualTo(2));
                        break;
                    case "2":
                        Assert.That(objects.Count(), Is.EqualTo(3));
                        break;
                    case "4":
                    case "5":
                    case "7":
                        Assert.That(objects.Count(), Is.EqualTo(1));
                        break;
                    default:
                        Assert.Fail("Unexpected group name: {0}", groupName);
                        break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void GroupByOneField()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GroupByTwoFields()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GroupByThreeFields()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GroupAndMaxAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void MaxAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GroupAndMinAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void MinAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GroupAndAverageAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void AverageAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GroupAndSumAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void SumAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GroupAndCountAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CountAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GroupAndContainsAggregate()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void ContainsAggregate()
        {
            throw new NotImplementedException();
        }

#if false
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
            var query = testTable.GroupBy(Query.Avg<TestObject>(to => to.SomeNumber), to => new { Name = to.Name });

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Item1.Name;
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
        #endif
    }
}
