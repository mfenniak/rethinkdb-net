using System;
using NUnit.Framework;
using FluentAssertions;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class DynamicTests : TestBase
    {
        private ITableQuery<dynamic> testTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.Run(Query.DbCreate("test"));
            connection.Run(Query.Db("test").TableCreate("table"));
            testTable = Query.Db("test").Table<dynamic>("table");
        }

        [SetUp]
        public virtual void SetUp()
        {
            /*
            connection.RunAsync(testTable.Insert(new List<TestObject> {
                new TestObject() { Id = "1", Name = "1", SomeNumber = 1, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C1" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C1" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C1" } } },
                new TestObject() { Id = "2", Name = "2", SomeNumber = 2, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "3", Name = "3", SomeNumber = 3, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C3" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C3" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C3" } } },
                new TestObject() { Id = "4", Name = "4", SomeNumber = 4, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "5", Name = "5", SomeNumber = 5, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C5" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C5" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C5" } } },
                new TestObject() { Id = "6", Name = "6", SomeNumber = 6, Tags = new[] { "even" }, Children = new TestObject[0], ChildrenList = new List<TestObject> { }, ChildrenIList = new List<TestObject> { } },
                new TestObject() { Id = "7", Name = "7", SomeNumber = 7, Tags = new[] { "odd" }, Children = new TestObject[] { new TestObject { Name = "C7" } }, ChildrenList = new List<TestObject> { new TestObject() { Name = "C7" } }, ChildrenIList = new List<TestObject> { new TestObject() { Name = "C7" } } },
            })).Wait();
            */
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.Run(testTable.Delete());
        }

        [Test]
        public void Test()
        {
            connection.Run(testTable.Insert(new { name = "Mathieu" }));

            int rowCount = 0;
            foreach (var obj in connection.Run(testTable))
            {
                // Should be able to access the property "name" just like we would have with the anonymous type.
                string name = obj.name;
                name.Should().Be("Mathieu");

                // Can also access the property by accessor, like it was a dictionary.
                name = obj["name"];
                name.Should().Be("Mathieu");

                rowCount += 1;
            }

            rowCount.Should().Be(1);
        }
    }
}
