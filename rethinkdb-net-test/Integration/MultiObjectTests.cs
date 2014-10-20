using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class MultiObjectTests : TestBase
    {
        private ITableQuery<TestObject> testTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
            testTable = Query.Db("test").Table<TestObject>("table");
            connection.Run(testTable.IndexCreate("index1", o => o.Name));
        }

        [SetUp]
        public virtual void SetUp()
        {
            connection.RunAsync(testTable.Insert(new List<TestObject> {
                new TestObject() { Id = "1", Name = "1", SomeNumber = 1, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C1" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C1" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C1" } } },
                new TestObject() { Id = "2", Name = "2", SomeNumber = 2, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "3", Name = "3", SomeNumber = 3, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C3" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C3" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C3" } } },
                new TestObject() { Id = "4", Name = "4", SomeNumber = 4, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "5", Name = "5", SomeNumber = 5, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C5" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C5" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C5" } } },
                new TestObject() { Id = "6", Name = "6", SomeNumber = 6, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "7", Name = "7", SomeNumber = 7, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C7" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C7" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C7" } } },
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
            Assert.That(count, Is.EqualTo(2));
            Assert.That(objects, Has.Count.EqualTo(2));

            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "2" }));
            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "3" }));
        }

        [Test]
        public void BetweenRightClosed()
        {
            DoBetweenRightClosed().Wait();
        }

        private async Task DoBetweenRightClosed()
        {
            var enumerable = connection.RunAsync(testTable.Between("2", "4", rightBound: Bound.Closed));
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
        public void BetweenLeftOpen()
        {
            DoBetweenLeftOpen().Wait();
        }

        private async Task DoBetweenLeftOpen()
        {
            var enumerable = connection.RunAsync(testTable.Between("2", "4", leftBound: Bound.Open));
            List<TestObject> objects = new List<TestObject>();
            var count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                objects.Add(enumerable.Current);
                ++count;
            }
            Assert.That(count, Is.EqualTo(1));
            Assert.That(objects, Has.Count.EqualTo(1));

            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "3" }));
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
            Assert.That(count, Is.EqualTo(3));
            Assert.That(objects, Has.Count.EqualTo(3));

            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "1" }));
            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "2" }));
            Assert.That(objects, Has.Exactly(1).EqualTo(new TestObject() { Id = "3" }));
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
            Assert.That(resp.Deleted, Is.EqualTo(3));
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
        public void UpdateClientSideEval()
        {
            var rand = new Random();
            var resp = connection.Run(testTable.Update(o => new TestObject() { SomeNumber = rand.NextDouble() }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            // "Replaced" seems weird here, rather than Updated, but that's what RethinkDB returns in the Data Explorer too...
            Assert.That(resp.Replaced, Is.EqualTo(7));

            // Verify that every record has the SAME value for SomeNumber, because rand.NextDouble() was evaluated client-side.
            double value = 0;
            bool first = true;
            foreach (var record in connection.Run(testTable))
            {
                if (first)
                {
                    value = record.SomeNumber;
                    first = false;
                }
                else
                    Assert.That(record.SomeNumber, Is.EqualTo(value));
            }
            Assert.That(first, Is.False);
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
            Assert.That(resp.Replaced, Is.EqualTo(3));
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
            Assert.That(resp, Is.EqualTo(3));
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
        public void FilterArrayLength()
        {
            DoFilterExpectedObjects(o => o.Children.Length > 0, new string[] { "1", "3", "5", "7" }).Wait();
        }

        [Test]
        public void FilterListCount()
        {
            DoFilterExpectedObjects(o => o.ChildrenList.Count > 0, new string[] { "1", "3", "5", "7" }).Wait();
        }

        [Test]
        public void FilterIListCount()
        {
            DoFilterExpectedObjects(o => o.ChildrenIList.Count > 0, new string[] { "1", "3", "5", "7" }).Wait();
        }

        private const string strValConst = "2";
        private static string strValStaticField = "2";
        private static string strValStaticProperty { get { return "2"; } }
        private string strValInstanceField = "2";
        private string strValInstanceProperty { get { return "2"; } }

        [Test]
        public void ParamExprVariableReferences()
        {
            string strVal = "2";
            DoFilterExpectedObjects(o => o.Name == strVal, "2").Wait();
        }

        [Test]
        public void ParamExprConstReferences()
        {
            DoFilterExpectedObjects(o => o.Name == strValConst, "2").Wait();
        }

        [Test]
        public void ParamExprStaticFieldReferences()
        {
            DoFilterExpectedObjects(o => o.Name == strValStaticField, "2").Wait();
        }

        [Test]
        public void ParamExprStaticPropertyReferences()
        {
            DoFilterExpectedObjects(o => o.Name == strValStaticProperty, "2").Wait();
        }

        [Test]
        public void ParamExprInstanceFieldReferences()
        {
            DoFilterExpectedObjects(o => o.Name == strValInstanceField, "2").Wait();
        }

        [Test]
        public void ParamExprInstancePropertyReferences()
        {
            DoFilterExpectedObjects(o => o.Name == strValInstanceProperty, "2").Wait();
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
            var enumerable = connection.RunAsync(testTable
                .OrderBy(o => o.Name)
                .ThenBy(o => o.Name)
                .ThenByDescending(o => o.Name)
            );
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
            var enumerable = connection.RunAsync(testTable.OrderBy(o => o.Name));
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
            var enumerable = connection.RunAsync(testTable.OrderByDescending(o => o.Name));
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
        public void OrderByDescPrimitive()
        {
            var enumerable = connection.Run(testTable.OrderByDescending(o => o.SomeNumber));
            var count = 0;
            foreach (var obj in enumerable)
            {
                ++count;
                Assert.That(obj.Name, Is.EqualTo((8 - count).ToString()));
            }
            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void OrderByIndexAsc()
        {
            // Order by field doesn't matter when you specify an index; "Tags" is used here even though we're sorting
            // on an index on Name, just to make sure that the index sort is actually being used.
            var enumerable = connection.Run(testTable.OrderBy(o => o.Tags, indexName: "index1"));
            var count = 0;
            foreach (var obj in enumerable)
            {
                ++count;
                Assert.That(obj.Name, Is.EqualTo(count.ToString()));
            }
            Assert.That(count, Is.EqualTo(7));
        }

        [Test]
        public void OrderByIndexDesc()
        {
            // Order by field doesn't matter when you specify an index; "Tags" is used here even though we're sorting
            // on an index on Name, just to make sure that the index sort is actually being used.
            var enumerable = connection.Run(testTable.OrderBy(o => o.Tags, direction: OrderByDirection.Descending, indexName: "index1"));
            var count = 0;
            foreach (var obj in enumerable)
            {
                ++count;
                Assert.That(obj.Name, Is.EqualTo((8 - count).ToString()));
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
            Assert.That(count, Is.EqualTo(1));
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

        [Test]
        public void MapReduceAnonymousTypes()
        {
            var retval = connection.Run(
                testTable
                    .Map(to => new { Value = to.SomeNumber, Count = 1.0 })
                    .Reduce((l, r) => new { Value = l.Value + r.Value, Count = l.Count + r.Count }));
            Assert.That(retval.Value, Is.EqualTo(28.0));
            Assert.That(retval.Count, Is.EqualTo(7.0));
        }

        [Test]
        public void ConcatMapArray()
        {
            var enumerable = connection.Run(
                testTable.ConcatMap(to => to.Children));
            int count = 0;
            foreach (var testObject in enumerable)
            {
                count++;
                Assert.That(testObject.Name, Is.StringStarting("C"));
            }
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public void ConcatMapList()
        {
            var enumerable = connection.Run(
                testTable.ConcatMap(to => to.ChildrenList));
            int count = 0;
            foreach (var testObject in enumerable)
            {
                count++;
                Assert.That(testObject.Name, Is.StringStarting("C"));
            }
            Assert.That(count, Is.EqualTo(4));
        }

        [Test]
        public void ConcatMapIList()
        {
            var enumerable = connection.Run(
                testTable.ConcatMap(to => to.ChildrenIList));
            int count = 0;
            foreach (var testObject in enumerable)
            {
                count++;
                Assert.That(testObject.Name, Is.StringStarting("C"));
            }
            Assert.That(count, Is.EqualTo(4));
        }

#if false
        [Test, Description("Tests that the SingleParameterLambda class can map a Parameter Expression (tag => tag) to a Term")]
        public void ConcatMap_OnSimpleDataType_CanUseParameterExpressionForQuery()
        {
            var query = testTable
                .ConcatMap(to => to.Tags)
                .GroupedMapReduce(
                    tag => tag,
                    tag => 1,
                    (l, r) => l+r);

            var enumerable = connection.Run(query);

            Assert.That(enumerable.Count(), Is.EqualTo(2));
            Assert.That(enumerable, Has.Member(Tuple.Create("even", 3)));
            Assert.That(enumerable, Has.Member(Tuple.Create("odd", 4)));
        }
#endif

        [Test]
        public void Union()
        {
            var enumerable = connection.Run(testTable.Union(testTable));
            int count = 0;
            foreach (var testObject in enumerable)
                count++;
            Assert.That(count, Is.EqualTo(14));
        }

        [Test]
        public void GetAll()
        {
            TestObject[] getAll = connection.Run(testTable.GetAll("3", "index1")).ToArray();
            Assert.That(getAll.Length, Is.EqualTo(1));
            Assert.That(getAll[0].Name, Is.EqualTo("3"));
        }

        [Test]
        public void GetAllNullIndex()
        {
            TestObject[] getAll = connection.Run(testTable.GetAll("3", null)).ToArray();
            Assert.That(getAll.Length, Is.EqualTo(1));
            Assert.That(getAll[0].Id, Is.EqualTo("3"));
        }

        [Test]
        public void GetAllEmptyIndex()
        {
            TestObject[] getAll = connection.Run(testTable.GetAll("3", "")).ToArray();
            Assert.That(getAll.Length, Is.EqualTo(1));
            Assert.That(getAll[0].Id, Is.EqualTo("3"));
        }

        [Test]
        public void BetweenIndex()
        {
            TestObject[] between = connection.Run(testTable.Between("3", "5", "index1")).ToArray();
            Assert.That(between.Length, Is.EqualTo(2));
            Assert.That(between.Single(o => o.Name == "3"), Is.Not.Null);
            Assert.That(between.Single(o => o.Name == "4"), Is.Not.Null);
        }

        [Test]
        public void BetweenIndexNull()
        {
            TestObject[] between = connection.Run(testTable.Between("3", "5", indexName: null)).ToArray();
            Assert.That(between.Length, Is.EqualTo(2));
            Assert.That(between.Single(o => o.Name == "3"), Is.Not.Null);
            Assert.That(between.Single(o => o.Name == "4"), Is.Not.Null);
        }

        [Test]
        public void BetweenIndexEmpty()
        {
            TestObject[] between = connection.Run(testTable.Between("3", "5", indexName: "")).ToArray();
            Assert.That(between.Length, Is.EqualTo(2));
            Assert.That(between.Single(o => o.Name == "3"), Is.Not.Null);
            Assert.That(between.Single(o => o.Name == "4"), Is.Not.Null);
        }

        [Test]
        public void LinqSyntaxWhere()
        {
            var q = from n in testTable where n.SomeNumber == 3 && n.SomeNumber != 4 select n;
            foreach (var obj in connection.Run(q))
            {
                Assert.That(obj.Id, Is.EqualTo("3"));
            }
        }

        [Test]
        public void LinqSyntaxSelect()
        {
            var q = from o in testTable select o.SomeNumber;
            double n = 0;
            foreach (double obj in connection.Run(q))
            {
                n += obj;
            }
            Assert.That(n, Is.EqualTo(28));
        }

        [Test]
        public void LinqSyntaxOrderBy()
        {
            var q = from o in testTable orderby o.SomeNumber select o;
            q = from o in testTable orderby o.SomeNumber ascending select o;
            q = from o in testTable orderby o.SomeNumber descending select o;
            foreach (var obj in connection.Run(q))
            {
            }
        }

        [Test]
        public void LinqSyntaxOrderByThenBy()
        {
            var q = from o in testTable orderby o.SomeNumber, o.Id select o;
            q = from o in testTable orderby o.SomeNumber, o.Id ascending select o;
            q = from o in testTable orderby o.SomeNumber, o.Id descending select o;
            foreach (var obj in connection.Run(q))
            {
            }
        }

        [Test]
        public void FilterOnChildArray()
        {
            DoFilterExpectedObjects(o => o.Children.Where(c => c.Name == "C1").Count() > 0, "1").Wait();
        }

        [Test]
        public void FilterOnChildList()
        {
            DoFilterExpectedObjects(o => o.ChildrenList.Where(c => c.Name == "C1").Count() > 0, "1").Wait();
        }

        [Test]
        public void FilterOnChildIList()
        {
            DoFilterExpectedObjects(o => o.ChildrenIList.Where(c => c.Name == "C1").Count() > 0, "1").Wait();
        }

        [Test]
        public void CountOnChildArray()
        {
            DoFilterExpectedObjects(o => o.Children.Count() == 1, "1", "3", "5", "7").Wait();
        }

        [Test]
        public void CountOnChildList()
        {
            DoFilterExpectedObjects(o => o.ChildrenList.Count() == 1, "1", "3", "5", "7").Wait();
        }

        [Test]
        public void CountOnChildIList()
        {
            DoFilterExpectedObjects(o => o.ChildrenIList.Count() == 1, "1", "3", "5", "7").Wait();
        }

        [Test]
        public void DateTimeAddDays()
        {
            var q = testTable.Select(t => new { n = t.SomeNumber, dt = new DateTime(2010, 1, 2, 3, 4, 5).AddDays(t.SomeNumber) });
            foreach (var rec in connection.Run(q))
            {
                Assert.That(rec.dt, Is.EqualTo(new DateTime(2010, 1, 2, 3, 4, 5).AddDays(rec.n)));
            }
        }

        [Test]
        public void DateTimeAddHours()
        {
            var q = testTable.Select(t => new { n = t.SomeNumber, dt = new DateTime(2010, 1, 2, 3, 4, 5).AddHours(t.SomeNumber) });
            foreach (var rec in connection.Run(q))
            {
                Assert.That(rec.dt, Is.EqualTo(new DateTime(2010, 1, 2, 3, 4, 5).AddHours(rec.n)));
            }
        }

        [Test]
        public void DateTimeAddMinutes()
        {
            var q = testTable.Select(t => new { n = t.SomeNumber, dt = new DateTime(2010, 1, 2, 3, 4, 5).AddMinutes(t.SomeNumber) });
            foreach (var rec in connection.Run(q))
            {
                Assert.That(rec.dt, Is.EqualTo(new DateTime(2010, 1, 2, 3, 4, 5).AddMinutes(rec.n)));
            }
        }

        [Test]
        public void DateTimeAddSeconds()
        {
            var q = testTable.Select(t => new { n = t.SomeNumber, dt = new DateTime(2010, 1, 2, 3, 4, 5).AddSeconds(t.SomeNumber) });
            foreach (var rec in connection.Run(q))
            {
                Assert.That(rec.dt, Is.EqualTo(new DateTime(2010, 1, 2, 3, 4, 5).AddSeconds(rec.n)));
            }
        }

        [Test]
        public void DateTimeAddMilliseconds()
        {
            var q = testTable.Select(t => new { n = t.SomeNumber, dt = new DateTime(2010, 1, 2, 3, 4, 5).AddMilliseconds(t.SomeNumber) });
            foreach (var rec in connection.Run(q))
            {
                // double seconds doesn't have quite the same precision as .NET's ticks; so we need to do a slightly
                // fuzzy comparison here; check that it's within 5 ticks, which is well within a millisecond
                // (10,000 ticks) of accuracy.
                Assert.That(rec.dt.Ticks, Is.EqualTo(new DateTime(2010, 1, 2, 3, 4, 5).AddMilliseconds(rec.n).Ticks).Within(5));
            }
        }

        [Test]
        public void DateTimeAddTicks()
        {
            // Unlike the other DateTimeAdd... tests, we don't reference t.SomeNumber here because it's a double and
            // we need a long; our expressions don't support numeric type casts because RethinkDB only supports one
            // numeric type, a double.
            var q = testTable.Select(t => new { n = t.SomeNumber, dt = new DateTime(2010, 1, 2, 3, 4, 5).AddTicks(5000000L) });
            foreach (var rec in connection.Run(q))
            {
                Assert.That(rec.dt, Is.EqualTo(new DateTime(2010, 1, 2, 3, 4, 5).AddTicks(5000000L)));
            }
        }
    }
}
