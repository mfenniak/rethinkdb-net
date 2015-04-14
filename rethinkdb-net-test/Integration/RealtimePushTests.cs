using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb;

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

        private void RealtimePushTestSingleResponse<T>(
            Func<IStreamingSequenceQuery<DmlResponseChange<T>>> createStreamingQuery,
            Action doModifications,
            Action<DmlResponseChange<T>> verifyStreamingResults)
        {
            Exception e1 = null;
            Exception e2 = null;

            ManualResetEvent sync1 = new ManualResetEvent(false);

            var thread1 = new Thread(() =>
            {
                try
                {
                    var query = createStreamingQuery();
                    IAsyncEnumerator<DmlResponseChange<T>> enumerator = null;
                    try
                    {
                        enumerator = connection.StreamChangesAsync(query);
                        var task = enumerator.MoveNext();
                        sync1.Set(); // inform other thread that we're ready for it to make changes
                        task.Wait();
                        task.Result.Should().BeTrue();

                        verifyStreamingResults(enumerator.Current);
                    }
                    finally
                    {
                        enumerator.Dispose().Wait();
                    }
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

        // Changes on a single record not currently supported by rethinkdb-net, but should be; issue #197.
        //[Test]
        //public void ChangesOnPrimaryKey()
        //{
        //    foreach (var row in testTable.Get("Id").Changes())
        //    {
        //    }
        //}

        [Test]
        [Timeout(1000)]
        public void ChangesWithTable()
        {
            RealtimePushTestSingleResponse(
                () => testTable.Changes(),
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

        [Test]
        [Timeout(1000)]
        public void ChangesWithFilter()
        {
            RealtimePushTestSingleResponse(
                () => testTable.Filter(o => o.Id == "3").Changes(),
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

        [Test]
        [Timeout(1000)]
        public void ChangesWithMap()
        {
            RealtimePushTestSingleResponse(
                () => testTable.Map(o => o.Name).Changes(),
                () =>
                {
                    var result = connection.Run(testTable.Get("3").Update(o => new TestObject() { Name = "Updated!" }));
                    result.Should().NotBeNull();
                    result.Replaced.Should().Be(1.0);
                },
                response =>
                {
                    response.OldValue.Should().Be("3");
                    response.NewValue.Should().Be("Updated!");
                }
            );
        }

        [Test]
        [Timeout(1000)]
        [Ignore("Fails due to RethinkDB error 'cannot call changes on an eager stream' despite RethinkdB 1.16 documentation claiming this should work")]
        public void ChangesWithOrderByLimit()
        {
            RealtimePushTestSingleResponse(
                () => testTable.OrderBy(o => o.SomeNumber, OrderByDirection.Descending).Limit(1).Changes(),
                () =>
                {
                    var result = connection.Run(testTable.Get("3").Update(o => new TestObject() { SomeNumber = 100 }));
                    result.Should().NotBeNull();
                    result.Replaced.Should().Be(1.0);
                },
                response =>
                {
                    response.OldValue.Id.Should().Be("7");
                    response.OldValue.SomeNumber.Should().Be(7);
                    response.NewValue.Id.Should().Be("3");
                    response.NewValue.SomeNumber.Should().Be(100);
                }
            );
        }

        /*
         * Min / Max operations on indexes not currently supported, but should be; issue #198

        [Test]
        [Timeout(1000)]
        public void ChangesWithMin()
        {
            RealtimePushTestSingleResponse(
                () => testTable.Min(someNumberIndex).Changes(),
                () =>
                {
                    var result = connection.Run(testTable.Get("3").Update(o => new TestObject() { SomeNumber = -100 }));
                    result.Should().NotBeNull();
                    result.Replaced.Should().Be(1.0);
                },
                response =>
                {
                    response.OldValue.Id.Should().Be("1");
                    response.OldValue.SomeNumber.Should().Be(1);
                    response.NewValue.Id.Should().Be("3");
                    response.NewValue.SomeNumber.Should().Be(-100);
                }
            );
        }

        [Test]
        [Timeout(1000)]
        public void ChangesWithMax()
        {
            RealtimePushTestSingleResponse(
                () => testTable.Max(someNumberIndex).Changes(),
                () =>
                {
                    var result = connection.Run(testTable.Get("3").Update(o => new TestObject() { SomeNumber = 100 }));
                    result.Should().NotBeNull();
                    result.Replaced.Should().Be(1.0);
                },
                response =>
                {
                    response.OldValue.Id.Should().Be("7");
                    response.OldValue.SomeNumber.Should().Be(7);
                    response.NewValue.Id.Should().Be("3");
                    response.NewValue.SomeNumber.Should().Be(100);
                }
            );
        }
        */
    }
}
