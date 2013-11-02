using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.Newtonsoft.Configuration;
using RethinkDb.Newtonsoft.Test.TestObjects;
using RethinkDb.Test.Integration;

namespace RethinkDb.Newtonsoft.Test.Integration
{
    [TestFixture]
    public class InsertTestObjectNewtonTests : TestBase
    {
        private ITableQuery<TestObjectNewton> testTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
        }

        [SetUp]
        public void BeforeEachTest()
        {
             testTable = Query.Db( "test" ).Table<TestObjectNewton>( "table" );
        }

        [TearDown]
        public void AfterEachTest()
        {
            connection.RunAsync(testTable.Delete()).Wait();
        }

        [Test]
        public void insert_testobjectnewton()
        {

            //INSERT
            var insertedObject = TestObjectNewtonWithTestData();
            var resp = connection.RunAsync(testTable.Insert(insertedObject));
            resp.Wait();


            //SELECT
            var obj = connection.RunAsync(testTable.Get(insertedObject.Id));
            obj.Wait();

            var result = obj.Result;
            result.Should().NotBeNull();
            result.Id.Should().Be(insertedObject.Id);

            //extra check / serialize everything to datum and compare
            //FluentAssertions doesn't check "field" equality, so 
            //converting everything to datums and THEN checking for equality
            //is best.
            var insertedDatum = DatumConvert.SerializeObject(insertedObject, ConfigurationAssembler.DefaultJsonSerializerSettings);
            var resultDatum = DatumConvert.SerializeObject(result, ConfigurationAssembler.DefaultJsonSerializerSettings);
            insertedDatum.ShouldBeEquivalentTo(resultDatum);

        }

        public TestObjectNewton TestObjectNewtonWithTestData()
        {
            var obj = new TestObjectNewton();
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