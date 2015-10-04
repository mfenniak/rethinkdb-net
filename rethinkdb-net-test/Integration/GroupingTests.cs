using System;
using NUnit.Framework;
using RethinkDb;
using System.Linq;
using System.Collections.Generic;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    [Ignore]
    public class GroupingTests : TestBase
    {
        private ITableQuery<TestObject> testTable;
        private IIndex<TestObject, string> nameIndex;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.Run(Query.DbCreate("test"));
            connection.Run(Query.Db("test").TableCreate("table"));

            testTable = Query.Db("test").Table<TestObject>("table");
            connection.Run(testTable.Insert(new TestObject[]
            {
                new TestObject() { Name = "1", SomeNumber = 1 },
                new TestObject() { Name = "1", SomeNumber = 1 },
                new TestObject() { Name = "2", SomeNumber = 2, Tags = new string[] { "A", "B" } },
                new TestObject() { Name = "2", SomeNumber = 200 },
                new TestObject() { Name = "2", SomeNumber = 2, Tags = new string[] { "A", "C" } },
                new TestObject() { Name = "3", SomeNumber = 3 },
                new TestObject() { Name = "3", SomeNumber = 3 },
                new TestObject() { Name = "4", SomeNumber = 4 },
                new TestObject() { Name = "5", SomeNumber = 5 },
                new TestObject() { Name = "6", SomeNumber = 6 },
                new TestObject() { Name = "6", SomeNumber = 6 },
                new TestObject() { Name = "7", SomeNumber = 7 },
            }));

            nameIndex = testTable.IndexDefine("name", to => to.Name);
            connection.Run(nameIndex.IndexCreate());
            connection.Run(nameIndex.IndexWait()).ToArray(); // ToArray ensures that the IEnumerable is actually evaluated completely and the wait is completed
        }

        [Test]
        public void GroupByIndex()
        {
            var query = testTable.Group(nameIndex);

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
            var query = testTable.Group(to => to.Name);

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
        public void GroupByTwoFields()
        {
            var query = testTable.Group(
                to => to.Name,
                to => to.SomeNumber
            );

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupKey = record.Key;
                var objects = record.Value;

                if (groupKey.Equals(Tuple.Create("1", 1d)) ||
                    groupKey.Equals(Tuple.Create("2", 2d)) ||
                    groupKey.Equals(Tuple.Create("3", 3d)) ||
                    groupKey.Equals(Tuple.Create("6", 6d)))
                {
                    Assert.That(objects.Count(), Is.EqualTo(2));
                }
                else if (
                    groupKey.Equals(Tuple.Create("2", 200d)) ||
                    groupKey.Equals(Tuple.Create("4", 4d)) ||
                    groupKey.Equals(Tuple.Create("5", 5d)) ||
                    groupKey.Equals(Tuple.Create("7", 7d)))
                {
                    Assert.That(objects.Count(), Is.EqualTo(1));
                }
                else
                {
                    Assert.Fail("Unexpected group key: {0}", groupKey);
                    break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(8));
        }

        [Test]
        public void GroupByThreeFields()
        {
            var query = testTable.Group(
                to => to.Name,
                to => to.SomeNumber,
                to => to.Tags
            );

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupKey = record.Key;
                var objects = record.Value;

                if (groupKey.Item1 == "2" && Math.Abs(groupKey.Item2 - 2d) <= Double.Epsilon)
                {
                    if (groupKey.Item3.SequenceEqual(new string[] { "A", "B" }) ||
                        groupKey.Item3.SequenceEqual(new string[] { "A", "C" }))
                    {
                        Assert.That(objects.Count(), Is.EqualTo(1));
                    }
                    else
                    {
                        Assert.Fail("Unexpected Tags on (2, 2): {0}", groupKey.Item3);
                    }
                }
                else if (groupKey.Equals(Tuple.Create<string, double, string[]>("1", 1d, null)) ||
                    groupKey.Equals(Tuple.Create<string, double, string[]>("3", 3d, null)) ||
                    groupKey.Equals(Tuple.Create<string, double, string[]>("6", 6d, null)))
                {
                    Assert.That(objects.Count(), Is.EqualTo(2));
                }
                else if (groupKey.Equals(Tuple.Create<string, double, string[]>("2", 200d, null)) ||
                    groupKey.Equals(Tuple.Create<string, double, string[]>("4", 4d, null)) ||
                    groupKey.Equals(Tuple.Create<string, double, string[]>("5", 5d, null)) ||
                    groupKey.Equals(Tuple.Create<string, double, string[]>("7", 7d, null)))
                {
                    Assert.That(objects.Count(), Is.EqualTo(1));
                }
                else
                {
                    Assert.Fail("Unexpected group key: {0}", groupKey);
                    break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(9));
        }

        [Test]
        public void GroupAndMaxAggregate()
        {
            var query = testTable.Group(nameIndex).Max(to => to.SomeNumber);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Key;
                var testObject = record.Value;

                switch (groupName)
                {
                    case "1":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(1));
                        break;
                    case "2":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(200));
                        break;
                    case "3":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(3));
                        break;
                    case "4":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(4));
                        break;
                    case "5":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(5));
                        break;
                    case "6":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(6));
                        break;
                    case "7":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(7));
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
        public void MaxAggregate()
        {
            var query = testTable.Max(to => to.SomeNumber);
            var testObject = connection.Run(query);
            Assert.That(testObject.SomeNumber, Is.EqualTo(200));
        }

        [Test]
        public void GroupAndMinAggregate()
        {
            var query = testTable.Group(nameIndex).Min(to => to.SomeNumber);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Key;
                var testObject = record.Value;

                switch (groupName)
                {
                    case "1":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(1));
                        break;
                    case "2":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(2));
                        break;
                    case "3":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(3));
                        break;
                    case "4":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(4));
                        break;
                    case "5":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(5));
                        break;
                    case "6":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(6));
                        break;
                    case "7":
                        Assert.That(testObject.SomeNumber, Is.EqualTo(7));
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
        public void MinAggregate()
        {
            var query = testTable.Min(to => to.SomeNumber);
            var testObject = connection.Run(query);
            Assert.That(testObject.SomeNumber, Is.EqualTo(1));
        }

        [Test]
        public void GroupAndAverageAggregate()
        {
            var query = testTable.Group(nameIndex).Avg(to => to.SomeNumber);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Key;
                var average = record.Value;

                switch (groupName)
                {
                    case "1":
                        Assert.That(average, Is.EqualTo(1));
                        break;
                    case "2":
                        Assert.That(average, Is.EqualTo(68));
                        break;
                    case "3":
                        Assert.That(average, Is.EqualTo(3));
                        break;
                    case "4":
                        Assert.That(average, Is.EqualTo(4));
                        break;
                    case "5":
                        Assert.That(average, Is.EqualTo(5));
                        break;
                    case "6":
                        Assert.That(average, Is.EqualTo(6));
                        break;
                    case "7":
                        Assert.That(average, Is.EqualTo(7));
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
        public void AverageAggregate()
        {
            var query = testTable.Avg(to => to.SomeNumber);
            var average = connection.Run(query);
            Assert.That(average, Is.EqualTo(20.0d));
        }

        [Test]
        public void GroupAndSumAggregate()
        {
            var query = testTable.Group(nameIndex).Sum(to => to.SomeNumber);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Key;
                var average = record.Value;

                switch (groupName)
                {
                    case "1":
                        Assert.That(average, Is.EqualTo(2));
                        break;
                    case "2":
                        Assert.That(average, Is.EqualTo(204));
                        break;
                    case "3":
                        Assert.That(average, Is.EqualTo(6));
                        break;
                    case "4":
                        Assert.That(average, Is.EqualTo(4));
                        break;
                    case "5":
                        Assert.That(average, Is.EqualTo(5));
                        break;
                    case "6":
                        Assert.That(average, Is.EqualTo(12));
                        break;
                    case "7":
                        Assert.That(average, Is.EqualTo(7));
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
        public void SumAggregate()
        {
            var query = testTable.Sum(to => to.SomeNumber);
            var average = connection.Run(query);
            Assert.That(average, Is.EqualTo(240));
        }

        [Test]
        public void GroupAndCountAggregate()
        {
            var query = testTable.Group(to => to.Name).Count(to => to.SomeNumber > 1);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Key;
                var objectCount = record.Value;

                switch (groupName)
                {
                    // Surprisingly missing; https://groups.google.com/forum/#!topic/rethinkdb/HXCHeTthF64
                    //case "1":
                    //    Assert.That(objectCount, Is.EqualTo(0));
                    //    break;
                    case "3":
                    case "6":
                        Assert.That(objectCount, Is.EqualTo(2));
                        break;
                    case "2":
                        Assert.That(objectCount, Is.EqualTo(3));
                        break;
                    case "4":
                    case "5":
                    case "7":
                        Assert.That(objectCount, Is.EqualTo(1));
                        break;
                    default:
                        Assert.Fail("Unexpected group name: {0}", groupName);
                        break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(6));
        }

        [Test]
        public void CountAggregate()
        {
            var count = connection.Run(testTable.Count(to => to.SomeNumber > 1));
            Assert.That(count, Is.EqualTo(10));
        }

        [Test]
        public void GroupAndContainsAggregate()
        {
            var query = testTable.Group(to => to.Name).Contains(to => to.SomeNumber > 1);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Key;
                var predicateResult = record.Value;

                switch (groupName)
                {
                    case "1":
                        Assert.That(predicateResult, Is.False);
                        break;
                    case "3":
                    case "2":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                        Assert.That(predicateResult, Is.True);
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
        public void ContainsAggregate()
        {
            var contains = connection.Run(testTable.Contains(to => to.SomeNumber > 1));
            Assert.That(contains, Is.True);
            contains = connection.Run(testTable.Contains(to => to.SomeNumber > 1000));
            Assert.That(contains, Is.False);
        }

        [Test]
        public void Ungroup()
        {
            var query = testTable
                .Group(to => to.Name)
                .Count()
                .Ungroup()
                .OrderBy(t => t.Reduction).ThenBy(t => t.Group);

            var result = connection.Run(query).ToArray();
            Assert.That(result, Has.Length.EqualTo(7));
            Assert.That(result[0].Group, Is.EqualTo("4")); Assert.That(result[0].Reduction, Is.EqualTo(1));
            Assert.That(result[1].Group, Is.EqualTo("5")); Assert.That(result[1].Reduction, Is.EqualTo(1));
            Assert.That(result[2].Group, Is.EqualTo("7")); Assert.That(result[2].Reduction, Is.EqualTo(1));
            Assert.That(result[3].Group, Is.EqualTo("1")); Assert.That(result[3].Reduction, Is.EqualTo(2));
            Assert.That(result[4].Group, Is.EqualTo("3")); Assert.That(result[4].Reduction, Is.EqualTo(2));
            Assert.That(result[5].Group, Is.EqualTo("6")); Assert.That(result[5].Reduction, Is.EqualTo(2));
            Assert.That(result[6].Group, Is.EqualTo("2")); Assert.That(result[6].Reduction, Is.EqualTo(3));
        }

        [Test]
        public void GroupedMapReduce()
        {
            // This is functionally the same as .Group().Sum(), but tests that Map and Reduce work on grouping queries.
            var query = testTable
                .Group(to => to.Name)
                .Map(to => to.SomeNumber)
                .Reduce((l, r) => l + r);

            int count = 0;
            foreach (var record in connection.Run(query))
            {
                var groupName = record.Key;
                var average = record.Value;

                switch (groupName)
                {
                    case "1":
                        Assert.That(average, Is.EqualTo(2));
                        break;
                    case "2":
                        Assert.That(average, Is.EqualTo(204));
                        break;
                    case "3":
                        Assert.That(average, Is.EqualTo(6));
                        break;
                    case "4":
                        Assert.That(average, Is.EqualTo(4));
                        break;
                    case "5":
                        Assert.That(average, Is.EqualTo(5));
                        break;
                    case "6":
                        Assert.That(average, Is.EqualTo(12));
                        break;
                    case "7":
                        Assert.That(average, Is.EqualTo(7));
                        break;
                    default:
                        Assert.Fail("Unexpected group name: {0}", groupName);
                        break;
                }

                ++count;
            }

            Assert.That(count, Is.EqualTo(7));
        }
    }
}
