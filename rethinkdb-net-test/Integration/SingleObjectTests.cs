using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using RethinkDb;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class SingleObjectTests : TestBase
    {
        private ITableQuery<TestObject> testTable;
        private TestObject insertedObject;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
        }

        [SetUp]
        public virtual void SetUp()
        {
            testTable = Query.Db("test").Table<TestObject>("table");
            DoInsert().Wait();
        }

        private async Task DoInsert()
        {
            insertedObject = new TestObject()
            {
                Name = "Jim Brown",
                Children = new TestObject[] {
                    new TestObject() { Name = "Scan" }
                },
                ChildrenList = new List<TestObject> {
                    new TestObject() { Name = "Scan" }
                },
                ChildrenIList = new List<TestObject> {
                    new TestObject() { Name = "Scan" }
                },
                SomeNumber = 1234,
                Data = new byte[] { 1, 2, 3, 4, 5 }
            };
            var resp = await connection.RunAsync(testTable.Insert(insertedObject));
            insertedObject.Id = resp.GeneratedKeys[0];
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync(testTable.Delete()).Wait();
        }

        [Test]
        public void GetQueryNull()
        {
            DoGetQueryNull().Wait();
        }

        private async Task DoGetQueryNull()
        {
            var obj = await connection.RunAsync(testTable.Get(insertedObject.Id));
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.Id, Is.EqualTo(insertedObject.Id));
        }

        [Test]
        public void Replace()
        {
            DoReplace().Wait();
        }

        private async Task DoReplace()
        {
            var resp = await connection.RunAsync(testTable.Get(insertedObject.Id).Replace(new TestObject() { Id = insertedObject.Id, Name = "Jack Black" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Replaced, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }

        [Test]
        public void ReplaceAndReturnValue()
        {
            var resp = connection.Run(testTable.Get(insertedObject.Id).ReplaceAndReturnChanges(new TestObject() { Id = insertedObject.Id, Name = "Jack Black" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Replaced, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
            Assert.That(resp.Changes, Is.Not.Null);
            Assert.That(resp.Changes, Has.Length.EqualTo(1));
            Assert.That(resp.Changes[0].OldValue, Is.Not.Null);
            Assert.That(resp.Changes[0].OldValue.Name, Is.EqualTo("Jim Brown"));
            Assert.That(resp.Changes[0].NewValue, Is.Not.Null);
            Assert.That(resp.Changes[0].NewValue.Name, Is.EqualTo("Jack Black"));
        }

        [Test]
        public void UpdateAndReturnValue()
        {
            var resp = connection.Run(testTable.Get(insertedObject.Id).UpdateAndReturnChanges(o => new TestObject() { Name = "Hello " + o.Id + "!" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Replaced, Is.EqualTo(1));

            Assert.That(resp.Changes, Is.Not.Null);
            Assert.That(resp.Changes, Has.Length.EqualTo(1));
            Assert.That(resp.Changes[0].NewValue, Is.Not.Null);
            Assert.That(resp.Changes[0].OldValue, Is.Not.Null);
            Assert.That(resp.Changes[0].OldValue.Name, Is.EqualTo("Jim Brown"));
            Assert.That(resp.Changes[0].NewValue.Name, Is.EqualTo("Hello " + resp.Changes[0].OldValue.Id + "!"));
        }

        [Test]
        public void UpdateAndReturnValueSequence()
        {
            var resp = connection.Run(testTable.UpdateAndReturnChanges(o => new TestObject() { Name = "Hello " + o.Id + "!" }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Replaced, Is.EqualTo(1));

            Assert.That(resp.Changes, Is.Not.Null);
            Assert.That(resp.Changes, Has.Length.EqualTo(1));
            Assert.That(resp.Changes[0].NewValue, Is.Not.Null);
            Assert.That(resp.Changes[0].OldValue, Is.Not.Null);
            Assert.That(resp.Changes[0].OldValue.Name, Is.EqualTo("Jim Brown"));
            Assert.That(resp.Changes[0].NewValue.Name, Is.EqualTo("Hello " + resp.Changes[0].OldValue.Id + "!"));
        }

        [Test]
        public void Delete()
        {
            DoDelete().Wait();
        }

        private async Task DoDelete()
        {
            var resp = await connection.RunAsync(testTable.Get(insertedObject.Id).Delete());
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Deleted, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }

        [Test]
        public void DeleteAndReturnValues()
        {
            var resp = connection.Run(testTable.Get(insertedObject.Id).DeleteAndReturnChanges());
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Deleted, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
            Assert.That(resp.Changes, Is.Not.Null);
            Assert.That(resp.Changes, Has.Length.EqualTo(1));
            Assert.That(resp.Changes[0].OldValue, Is.Not.Null);
            Assert.That(resp.Changes[0].OldValue.Id, Is.EqualTo(insertedObject.Id));
            Assert.That(resp.Changes[0].NewValue, Is.Null);
        }

        [Test]
        public void DeleteAndReturnValuesSequence()
        {
            var resp = connection.Run(testTable.DeleteAndReturnChanges());
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Deleted, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
            Assert.That(resp.Changes, Is.Not.Null);
            Assert.That(resp.Changes, Has.Length.EqualTo(1));
            Assert.That(resp.Changes[0].OldValue, Is.Not.Null);
            Assert.That(resp.Changes[0].OldValue.Id, Is.EqualTo(insertedObject.Id));
            Assert.That(resp.Changes[0].NewValue, Is.Null);
        }

        [Test]
        public void GetUpdateNumericAdd()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber + 1 }, 1235).Wait();
        }

        [Test]
        public void GetUpdateNumericSub()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber - 1 }, 1233).Wait();
        }

        [Test]
        public void GetUpdateNumericDiv()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber / 2 }, 617).Wait();
        }

        [Test]
        public void GetUpdateNumericMul()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber * 2 }, 2468).Wait();
        }

        [Test]
        public void GetUpdateNumericMod()
        {
            DoGetUpdateNumeric(o => new TestObject() { SomeNumber = o.SomeNumber % 600 }, 34).Wait();
        }

        private async Task DoGetUpdateNumeric(Expression<Func<TestObject, TestObject>> expr, double expected)
        {
            var resp = await connection.RunAsync(testTable.Get(insertedObject.Id).Update(expr));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            // "Replaced" seems weird here, rather than Updated, but that's what RethinkDB returns in the Data Explorer too...
            Assert.That(resp.Replaced, Is.EqualTo(1));

            var obj = await connection.RunAsync(testTable.Get(insertedObject.Id));
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.SomeNumber, Is.EqualTo(expected));
        }

        [Test]
        public void Reduce()
        {
            DoReduce().Wait();
        }

        private async Task DoReduce()
        {
            var resp = await connection.RunAsync(testTable.Reduce((acc, val) => new TestObject() { SomeNumber = acc.SomeNumber + val.SomeNumber }));
            Assert.That(resp.SomeNumber, Is.EqualTo(1234));
        }

        [Test]
        public void ReduceToPrimitive()
        {
            DoReduceToPrimitive().Wait();
        }

        private async Task DoReduceToPrimitive()
        {
            var resp = await connection.RunAsync(testTable.Map(o => o.SomeNumber).Reduce((acc, val) => acc + val));
            Assert.That(resp, Is.EqualTo(1234));
        }

        [Test]
        public void SliceOneArg()
        {
            // 1-arg slice
            var slice = connection.Run(testTable.Map(testObject => testObject.Data.Slice(3))).Single();
            Assert.That(slice, Is.EquivalentTo(new byte[] { 4, 5 }));
        }

        [Test]
        public void SliceTwoArg()
        {
            // 2-arg slice
            var slice = connection.Run(testTable.Map(testObject => testObject.Data.Slice(0, 1))).Single();
            Assert.That(slice, Is.EquivalentTo(new byte[] { 1 }));
        }

        [Test]
        public void SliceFourArg()
        {
            // 4-arg slice
            var slice = connection.Run(testTable.Map(testObject => testObject.Data.Slice(0, 2, Bound.Open, Bound.Closed))).Single();
            Assert.That(slice, Is.EquivalentTo(new byte[] { 2, 3 }));
        }
    }
}
