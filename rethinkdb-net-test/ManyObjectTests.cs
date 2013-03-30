using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.QueryTerm;
using System.Collections.Generic;

namespace RethinkDb.Test
{
    [TestFixture]
    public class ManyObjectTests : TestBase
    {
        private TableQuery<TestObject> testTable;

        [SetUp]
        public virtual void SetUp()
        {
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
            testTable = Query.Db("test").Table<TestObject>("table");

            // Insert more than 1000 objects to test the enumerable loading additional chunks of the sequence
            var objectList = new List<TestObject>();
            for (int i = 0; i < 1005; i++)
                objectList.Add(new TestObject() { Name = "Object #" + i });
            connection.RunAsync(testTable.Insert(objectList)).Wait();
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync(Query.DbDrop("test")).Wait();
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
    }
}

