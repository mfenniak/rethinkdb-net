using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.Newtonsoft.Configuration;
using RethinkDb.Newtonsoft.Test.TestObjects;
using RethinkDb.Test.Integration;

namespace RethinkDb.Newtonsoft.Test.Integration
{
    [TestFixture]
    public class ComplexObjectTests : TestBase
    {
        static ComplexObjectTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }

        private ITableQuery<ComplexObject> testTable;
        private ComplexObject insertedObject;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.RunAsync( Query.DbCreate( "test" ) ).Wait();
            connection.RunAsync( Query.Db( "test" ).TableCreate( "table" ) ).Wait();
        }

        [SetUp]
        public virtual void SetUp()
        {
            testTable = Query.Db( "test" ).Table<ComplexObject>( "table" );
            DoInsert().Wait();
        }

        private async Task DoInsert()
        {
            insertedObject = new ComplexObject
                {
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
                    Signature = new byte[] {0xde, 0xad, 0xbe, 0xef},
                    Hours = new[] {1, 2, 3, 4},
                    ExtraInfo = new Dictionary<string, string>()
                        {
                            {"key1", "value1"},
                            {"key2", "value2"},
                            {"key3", "value3"},
                        },
                    Enabled = true,
                    Notify = null,
                    BinaryBools = new[] {true, false, true},
                    NullBinaryBools = new bool?[] {true, null, true},
                    SomeNumber = 1234
                };

            var resp = await connection.RunAsync( testTable.Insert( insertedObject ) );
            insertedObject.Id = resp.GeneratedKeys[0];
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync( testTable.Delete() ).Wait();
        }

        [Test]
        public void GetQueryNull()
        {
            DoGetQueryNull().Wait();
        }

        private async Task DoGetQueryNull()
        {
            var obj = await connection.RunAsync( testTable.Get( insertedObject.Id ) );
            Assert.That( obj, Is.Not.Null );
            Assert.That( obj.Id, Is.EqualTo( insertedObject.Id ) );
        }

        [Test]
        public void Replace()
        {
            DoReplace().Wait();
        }

        private async Task DoReplace()
        {
            var resp = await connection.RunAsync( testTable.Get( insertedObject.Id ).Replace( new ComplexObject() { Id = insertedObject.Id, Name = "Jack Black" } ) );
            Assert.That( resp, Is.Not.Null );
            Assert.That( resp.FirstError, Is.Null );
            Assert.That( resp.Replaced, Is.EqualTo( 1 ) );
            Assert.That( resp.GeneratedKeys, Is.Null );
        }

        [Test]
        public void ReplaceAndReturnValue()
        {
            var resp = connection.Run( testTable.Get( insertedObject.Id ).ReplaceAndReturnValue( new ComplexObject() { Id = insertedObject.Id, Name = "Jack Black" } ) );
            Assert.That( resp, Is.Not.Null );
            Assert.That( resp.FirstError, Is.Null );
            Assert.That( resp.Replaced, Is.EqualTo( 1 ) );
            Assert.That( resp.GeneratedKeys, Is.Null );
            Assert.That( resp.OldValue, Is.Not.Null );
            Assert.That( resp.OldValue.Name, Is.EqualTo( "Brian Chavez" ) );
            Assert.That( resp.NewValue, Is.Not.Null );
            Assert.That( resp.NewValue.Name, Is.EqualTo( "Jack Black" ) );
        }

        [Test]
        public void UpdateAndReturnValue()
        {
            var resp = connection.Run( testTable.Get( insertedObject.Id ).UpdateAndReturnValue( o => new ComplexObject() { Name = "Hello " + o.Id + "!" } ) );
            Assert.That( resp, Is.Not.Null );
            Assert.That( resp.FirstError, Is.Null );
            Assert.That( resp.Replaced, Is.EqualTo( 1 ) );

            Assert.That( resp.NewValue, Is.Not.Null );
            Assert.That( resp.OldValue, Is.Not.Null );

            Assert.That( resp.OldValue.Name, Is.EqualTo( "Brian Chavez" ) );
            Assert.That( resp.NewValue.Name, Is.EqualTo( "Hello " + resp.OldValue.Id + "!" ) );
        }

        [Test]
        public void Delete()
        {
            DoDelete().Wait();
        }

        private async Task DoDelete()
        {
            var resp = await connection.RunAsync( testTable.Get( insertedObject.Id ).Delete() );
            Assert.That( resp, Is.Not.Null );
            Assert.That( resp.FirstError, Is.Null );
            Assert.That( resp.Deleted, Is.EqualTo( 1 ) );
            Assert.That( resp.GeneratedKeys, Is.Null );
        }

        [Test]
        public void DeleteAndReturnValues()
        {
            var resp = connection.Run( testTable.Get( insertedObject.Id ).DeleteAndReturnValue() );
            Assert.That( resp, Is.Not.Null );
            Assert.That( resp.FirstError, Is.Null );
            Assert.That( resp.Deleted, Is.EqualTo( 1 ) );
            Assert.That( resp.GeneratedKeys, Is.Null );
            Assert.That( resp.OldValue, Is.Not.Null );
            Assert.That( resp.OldValue.Id, Is.EqualTo( insertedObject.Id ) );
            Assert.That( resp.NewValue, Is.Null );
        }

        [Test]
        public void GetUpdateNumericAdd()
        {
            DoGetUpdateNumeric( o => new ComplexObject() { SomeNumber = o.SomeNumber + 1 }, 1235 ).Wait();
        }

        [Test]
        public void GetUpdateNumericSub()
        {
            DoGetUpdateNumeric( o => new ComplexObject()  { SomeNumber = o.SomeNumber - 1 }, 1233 ).Wait();
        }

        [Test]
        public void GetUpdateNumericDiv()
        {
            DoGetUpdateNumeric( o => new ComplexObject() { SomeNumber = o.SomeNumber / 2 }, 617 ).Wait();
        }

        [Test]
        public void GetUpdateNumericMul()
        {
            DoGetUpdateNumeric( o => new ComplexObject() { SomeNumber = o.SomeNumber * 2 }, 2468 ).Wait();
        }

        [Test]
        public void GetUpdateNumericMod()
        {
            DoGetUpdateNumeric( o => new ComplexObject() { SomeNumber = o.SomeNumber % 600 }, 34 ).Wait();
        }

        private async Task DoGetUpdateNumeric( Expression<Func<ComplexObject, ComplexObject>> expr, double expected )
        {
            var resp = await connection.RunAsync( testTable.Get( insertedObject.Id ).Update( expr ) );
            Assert.That( resp, Is.Not.Null );
            Assert.That( resp.FirstError, Is.Null );
            // "Replaced" seems weird here, rather than Updated, but that's what RethinkDB returns in the Data Explorer too...
            Assert.That( resp.Replaced, Is.EqualTo( 1 ) );

            var obj = await connection.RunAsync( testTable.Get( insertedObject.Id ) );
            Assert.That( obj, Is.Not.Null );
            Assert.That( obj.SomeNumber, Is.EqualTo( expected ) );
        }

        [Test]
        public void Reduce()
        {
            DoReduce().Wait();
        }

        private async Task DoReduce()
        {
            var resp = await connection.RunAsync( testTable.Reduce( ( acc, val ) => new ComplexObject() { SomeNumber = acc.SomeNumber + val.SomeNumber } ) );
            Assert.That( resp.SomeNumber, Is.EqualTo( 1234 ) );
        }

        [Test]
        public void ReduceToPrimitive()
        {
            DoReduceToPrimitive().Wait();
        }

        private async Task DoReduceToPrimitive()
        {
            var resp = await connection.RunAsync( testTable.Map( o => o.SomeNumber ).Reduce( ( acc, val ) => acc + val ) );
            Assert.That( resp, Is.EqualTo( 1234 ) );
        }
    }

}