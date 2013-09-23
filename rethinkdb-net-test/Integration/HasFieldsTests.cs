using System.Linq;
using NUnit.Framework;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class HasFieldsTests : TestBase
    {
        private ITableQuery<TestObject> testTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
            testTable = Query.Db("test").Table<TestObject>("table");
            connection.Run(testTable.IndexCreate("index1", o => o.Name));
        }

        [SetUp]
        public virtual void SetUp()
        {
            connection.RunAsync(testTable.Insert(new TestObject[] {
                new TestObject() { Id = "1", Name = null, Children = new TestObject[] { }},
                new TestObject() { Id = "2", Name = "2", Children = new TestObject[] { }},
                new TestObject() { Id = "3", Name = null, Children = null },
                new TestObject() { Id = "4", Name = string.Empty, Children = null }
            })).Wait();
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync(testTable.Delete()).Wait();
        }

        [Test]
        public void HasFields_OnSequence_ReturnsResultsWithNonNullFieldValues()
        {
            TestObject[] hasFields = connection.Run(testTable.HasFields(m => m.Name)).ToArray();
            
            Assert.That(hasFields.Length, Is.EqualTo(2));
            Assert.That(hasFields, Has.Exactly(1).EqualTo(new TestObject { Id = "2" }));
            Assert.That(hasFields, Has.Exactly(1).EqualTo(new TestObject { Id = "4" }));
        }

        [Test]
        public void HasFields_OnSequence_ReturnsResultsWithMultipleNonNullNamesAndChildren()
        {
            TestObject[] hasFields = connection.Run(testTable.HasFields(m => m.Name, m => m.Children)).ToArray();
            
            Assert.That(hasFields.Length, Is.EqualTo(1));
            Assert.That(hasFields, Has.Exactly(1).EqualTo(new TestObject { Id = "2" }));
        }

        [Test]
        public void HasFields_OnSingleObject_ReturnsFalseWhenFieldIsNull()
        {
            var result = this.connection.Run(testTable.Get("1").HasFields(m => m.Name));
            Assert.That(result, Is.False);
        }

        [Test]
        public void HasFields_OnSingleObject_ReturnsTrueWhenFieldIsNotNull()
        {
            var result = connection.Run(testTable.Get("1").HasFields(m => m.Children));
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasFields_OnSingleObject_ReturnsTrueWhenAllFieldsAreNotNull()
        {
            var result = connection.Run(testTable.Get("1").HasFields(m => m.Children, m => m.Id));
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasFields_OnSingleObject_ReturnsFalseWhenSomeFieldIsNull()
        {
            var result = connection.Run(testTable.Get("1").HasFields(m => m.Children, m => m.Name));
            Assert.That(result, Is.False);
        }
    }
}