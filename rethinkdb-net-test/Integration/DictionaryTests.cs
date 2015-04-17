using System;
using NUnit.Framework;
using RethinkDb;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class DictionaryTests : TestBase
    {
        private ITableQuery<TestObjectWithDictionary> testTable;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            connection.Run(Query.DbCreate("test"));
            connection.Run(Query.Db("test").TableCreate("table"));
            testTable = Query.Db("test").Table<TestObjectWithDictionary>("table");
        }

        [SetUp]
        public virtual void SetUp()
        {
            connection.Run(
                testTable.Insert(
                    new[] {
                        new TestObjectWithDictionary()
                        {
                            Name = "Jack Black",
                            FreeformProperties = new Dictionary<string, object>()
                            {
                                { "Awesome-Level", 100 },
                                { "Cool-Level", 15 },
                                { "Best Movie", "School of Rock" }
                            }
                        },
                        new TestObjectWithDictionary()
                        {
                            Name = "Gil Grissom",
                            FreeformProperties = new Dictionary<string, object>()
                            {
                                { "Awesome-Level", 101 },
                                { "Cool-Level", 0 },
                                { "Best Known For", "CSI: Las Vegas" }
                            }
                        },
                        new TestObjectWithDictionary()
                        {
                            Name = "Madame Curie",
                            FreeformProperties = new Dictionary<string, object>()
                            {
                                { "Awesome-Level", 15 },
                                { "Cool-Level", -1 },
                                { "Impressive", true }
                            }
                        }
                    }
                )
            );
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.Run(testTable.Delete());
        }

        [Test]
        public void ContainsKey()
        {
            var enumerable = connection.Run(testTable.Map(o => o.FreeformProperties.ContainsKey("Best Movie")));
            var numTrue = enumerable.Count(r => r == true);
            var numFalse = enumerable.Count(r => r == false);
            numTrue.Should().Be(1);
            numFalse.Should().Be(2);
        }
    }
}
