using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.Newtonsoft.Configuration;
using RethinkDb.Newtonsoft.Test.TestObjects;
using RethinkDb.Test.Integration;

namespace RethinkDb.Newtonsoft.Test.Integration
{
    [TestFixture]
    public class InsertComplexObjectTests : TestBase
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
            testTable = Query.Db("test").Table<TestObjectNewton>("table");
        }

        [TearDown]
        public void AfterEachTest()
        {
            connection.RunAsync(testTable.Delete()).Wait();
        }


        [Test]
        public void insert_complexobject()
        {
            var testTable = Query.Db( "test" ).Table<ComplexObject>( "table" );

            //INSERT
            var insertedObject = ComplexObjectWithDefaults();
            var resp = connection.RunAsync( testTable.Insert( insertedObject ) );
            resp.Wait();


            //SELECT
            var obj = connection.RunAsync( testTable.Get( insertedObject.Id.ToString() ) );
            obj.Wait();

            var result = obj.Result;
            result.Should().NotBeNull();
            result.Id.Should().Be( insertedObject.Id );


            var insertedDatum = DatumConvert.SerializeObject( insertedObject, ConfigurationAssembler.DefaultJsonSerializerSettings );
            var resultDatum = DatumConvert.SerializeObject( result, ConfigurationAssembler.DefaultJsonSerializerSettings );
            insertedDatum.ShouldBeEquivalentTo( resultDatum );
        }

        public static ComplexObject ComplexObjectWithDefaults()
        {
            var obj = new ComplexObject
                {
                    Id = Guid.Parse( "{32753EDC-E5EF-46E0-ABCD-CE5413B30797}" ),
                    Name = "Brian Chavez",
                    ProfileUri = new Uri( "http://www.bitarmory.com" ),
                    CompanyUri = null,
                    Balance = 1000001.2m,
                    Clicks = 2000,
                    Views = null,
                    SecurityStamp = Guid.Parse( "32753EDC-E5EF-46E0-ABCD-CE5413B30797" ),
                    TrackingId = null,
                    LastLogin = new DateTime( 2013, 1, 14, 4, 44, 25 ),
                    LoginWindow = new TimeSpan( 1, 2, 3, 4, 5 ),
                    Signature = new byte[] { 0xde, 0xad, 0xbe, 0xef },
                    Hours = new[] { 1, 2, 3, 4 },
                    ExtraInfo = new Dictionary<string, string>()
                        {
                            {"key1", "value1"},
                            {"key2", "value2"},
                            {"key3", "value3"},
                        },
                    Enabled = true,
                    Notify = null,
                    BinaryBools = new[] { true, false, true },
                    NullBinaryBools = new bool?[] { true, null, true }
                };
            return obj;
        }
    }
}