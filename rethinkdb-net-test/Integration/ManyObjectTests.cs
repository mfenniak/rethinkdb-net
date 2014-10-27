using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class ManyObjectTests : TestBase
    {
        private ITableQuery<TestObject> testTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
            testTable = Query.Db("test").Table<TestObject>("table");

            // Insert more than 1000 objects to test the enumerable loading additional chunks of the sequence
            var objectList = new List<TestObject>();
            for (int i = 0; i < 1005; i++)
                objectList.Add(new TestObject() { Name = "Object #" + i });
            connection.RunAsync(testTable.Insert(objectList)).Wait();
        }

        public override void TestFixtureTearDown()
        {
            connection.RunAsync(Query.DbDrop("test")).Wait();

            base.TestFixtureTearDown();
        }

        [Test]
        public void StreamingEnumerator()
        {
            DoStreamingEnumerator().Wait();
        }

        private async Task DoStreamingEnumerator()
        {
            var enumerable = connection.RunAsync(testTable);
            int count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
            }
            Assert.That(count, Is.EqualTo(1005));
        }

        [Test]
        public void AbortAsyncStreamingEnumerator()
        {
            DoAbortAsyncStreamingEnumerator().Wait();
        }

        private async Task DoAbortAsyncStreamingEnumerator()
        {
            var enumerable = connection.RunAsync(testTable);
            int count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
                if (count > 10)
                    break;
            }
            // not really sure if there's anything that can be asserted here, so we're just testing
            // if Dispose succeeds without exceptions.  Technically doesn't really test that the query was
            // stopped on the server-side like we'd like to test.
            await enumerable.Dispose();
        }

        [Test]
        public void AbortSynchronousStreamingEnumerator()
        {
            int count = 0;
            foreach (var record in connection.Run(testTable))
            {
                count++;
                if (count > 10)
                    break;
            }
            // not really sure if there's anything that can be asserted here, so we're just testing
            // if Dispose succeeds without exceptions.  Technically doesn't really test that the query was
            // stopped on the server-side like we'd like to test.
        }

        [Test]
        public void ReuseSynchronousEnumerable()
        {
            var enumerable = connection.Run(testTable);
            for (int i = 0; i < 5; i++)
            {
                int count = 0;
                foreach (var record in enumerable)
                    ++count;
                Assert.That(count, Is.EqualTo(1005));
            }
        }

        [Test]
        public void Sample()
        {
            var query = testTable.Sample(10);

            int count = 0;
            HashSet<string> firstRunIds = new HashSet<string>();
            foreach (var o in connection.Run(query))
            {
                firstRunIds.Add(o.Id);
                count += 1;
            }
            Assert.That(count, Is.EqualTo(10));

            HashSet<string> secondRunIds = new HashSet<string>();
            foreach (var o in connection.Run(query))
            {
                secondRunIds.Add(o.Id);
                count += 1;
            }
            Assert.That(count, Is.EqualTo(20));

            // Two random sets of ten objects shouldn't have a lot of the same objects, if any; otherwise our Sample
            // query might not be doing quite what we expect...
            firstRunIds.IntersectWith(secondRunIds);
            Assert.That(firstRunIds.Count, Is.LessThan(5));
        }
    }
}

