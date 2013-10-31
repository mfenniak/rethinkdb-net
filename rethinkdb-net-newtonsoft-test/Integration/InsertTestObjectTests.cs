using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.Test.Integration;

namespace RethinkDb.Newtonsoft.Test.Integration
{
    [TestFixture]
    public class InsertTestObjectTests : TestBase
    {
        private ITableQuery<TestObject> testTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
        }

        [SetUp]
        public void BeforeEachTest()
        {
            testTable = Query.Db("test").Table<TestObject>("table");
        }

        [TearDown]
        public void AfterEachTest()
        {
            connection.RunAsync(testTable.Delete()).Wait();
        }

        [Test]
        public void insert_testobject()
        {
            //INSERT
            var insertedObject = TestObjectWithTestData();
            var resp = connection.RunAsync( testTable.Insert( insertedObject ) );
            resp.Wait();


            //SELECT
            var obj = connection.RunAsync( testTable.Get( insertedObject.Id ) );
            obj.Wait();

            var result = obj.Result;

            result.Should().NotBeNull();
            result.Id.Should().Be( insertedObject.Id );

            var insertedDatum = DatumConvert.SerializeObject( insertedObject, NewtonsoftDatumConverterFactory.DefaultSeralizerSettings );
            var resultDatum = DatumConvert.SerializeObject( result, NewtonsoftDatumConverterFactory.DefaultSeralizerSettings );
            insertedDatum.ShouldBeEquivalentTo( resultDatum );
        }

        public TestObject TestObjectWithTestData()
        {
            var obj = new TestObject();
            obj.Id = "my_id_1234";
            obj.Name = "Brian Chavez";
            obj.SomeNumber = 1234.5;
            obj.Tags = new[] { "tag1", "tag2", "tag3" };

            obj.Children = new[]
                {
                    new TestObject
                        {
                            Name = "childrenArray1"
                        },
                    new TestObject
                        {
                            Name = "childrenArray2"
                        },
                    new TestObject
                        {
                            Name = "childrenArray3"
                        }
                };

            obj.ChildrenList = new List<TestObject>()
                {
                    new TestObject
                        {
                            Name = "childList1"
                        },
                    new TestObject
                        {
                            Name = "childList2"
                        },
                    new TestObject
                        {
                            Name = "childList3"
                        }
                };

            obj.ChildrenIList = new List<TestObject>()
                {
                    new TestObject
                        {
                            Name = "childrenIList1"
                        },
                    new TestObject
                        {
                            Name = "childrenIList2"
                        },
                    new TestObject
                        {
                            Name = "childrenIList3"
                        }
                };

            return obj;
        }

    }
}