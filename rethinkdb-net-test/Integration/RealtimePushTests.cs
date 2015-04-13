using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class RealtimePushTests : TestBase
    {
        private ITableQuery<TestObject> testTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.Run(Query.DbCreate("test"));
            connection.Run(Query.Db("test").TableCreate("table"));
            testTable = Query.Db("test").Table<TestObject>("table");
        }

        [SetUp]
        public virtual void SetUp()
        {
            connection.Run(testTable.Insert(new List<TestObject> {
                new TestObject() { Id = "1", Name = "1", SomeNumber = 1, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C1" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C1" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C1" } } },
                new TestObject() { Id = "2", Name = "2", SomeNumber = 2, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "3", Name = "3", SomeNumber = 3, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C3" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C3" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C3" } } },
                new TestObject() { Id = "4", Name = "4", SomeNumber = 4, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "5", Name = "5", SomeNumber = 5, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C5" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C5" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C5" } } },
                new TestObject() { Id = "6", Name = "6", SomeNumber = 6, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "7", Name = "7", SomeNumber = 7, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C7" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C7" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C7" } } },
            }));
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.Run(testTable.Delete());
        }

        //[Test]
        //public void ChangesOnPrimaryKey()
        //{
        //    foreach (var row in testTable.Get("Id").Changes())
        //    {
        //    }
        //}

        private void RealtimePushTestSingleResponse(
            Func<IStreamingSequenceQuery<DmlResponseChange<TestObject>>> createStreamingQuery,
            Action doModifications,
            Action<DmlResponseChange<TestObject>> verifyStreamingResults)
        {
            Exception e1 = null;
            Exception e2 = null;

            ManualResetEvent sync1 = new ManualResetEvent(false);

            var thread1 = new Thread(() =>
            {
                try
                {
                    var query = createStreamingQuery();
                    var enumerator = connection.StreamChangesAsync(query);
                    var task = enumerator.MoveNext();
                    sync1.Set(); // inform other thread that we're ready for it to make changes
                    task.Wait();
                    task.Result.Should().BeTrue();

                    verifyStreamingResults(enumerator.Current);
                }
                catch (Exception e)
                {
                    e1 = e;
                }
            });

            var thread2 = new Thread(() =>
            {
                try
                {
                    sync1.WaitOne();
                    doModifications();
                }
                catch (Exception e)
                {
                    e2 = e;
                }
            });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            e1.Should().BeNull();
            e2.Should().BeNull();
        }

        [Test]
        [Timeout(1000)]
        public void ChangesWithBetween()
        {
            RealtimePushTestSingleResponse(
                () => testTable.Between("2", "4").Changes(),
                () =>
                {
                    var result = connection.Run(testTable.Get("3").Update(o => new TestObject() { Name = "Updated!" }));
                    result.Should().NotBeNull();
                    result.Replaced.Should().Be(1.0);
                },
                response =>
                {
                    response.OldValue.Name.Should().Be("3");
                    response.NewValue.Name.Should().Be("Updated!");
                }
            );
        }
    }
}
