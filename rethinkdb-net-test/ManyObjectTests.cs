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
            connection.Run(Query.DbCreate("test")).Wait();
            connection.Run(Query.Db("test").TableCreate("table")).Wait();
            testTable = Query.Db("test").Table<TestObject>("table");

            // Insert more than 1000 objects to test the enumerable loading additional chunks of the sequence
            var objectList = new List<TestObject>();
            for (int i = 0; i < 1005; i++)
                objectList.Add(new TestObject() { Name = "Object #" + i });
            connection.Run(testTable.Insert(objectList)).Wait();
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.Run(Query.DbDrop("test")).Wait();
        }

        [Test]
        public void StreamingEnumerator()
        {
            DoStreamingEnumerator().Wait();
        }

        private async Task DoStreamingEnumerator()
        {
            var enumerable = connection.Run(testTable);
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

