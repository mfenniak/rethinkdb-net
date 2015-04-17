using System;
using NUnit.Framework;
using RethinkDb;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class NamedValueDictionaryTests : TestBase
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
                                { "awesome level", 100 },
                                { "cool level", 15 },
                                { "best movie", "School of Rock" }
                            }
                        },
                        new TestObjectWithDictionary()
                        {
                            Name = "Gil Grissom",
                            FreeformProperties = new Dictionary<string, object>()
                            {
                                { "awesome level", 101 },
                                { "cool level", 0 },
                                { "best known for", "CSI: Las Vegas" }
                            }
                        },
                        new TestObjectWithDictionary()
                        {
                            Name = "Madame Curie",
                            FreeformProperties = new Dictionary<string, object>()
                            {
                                { "awesome level", 15 },
                                { "cool level", -1 },
                                { "impressive", true }
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
            var enumerable = connection.Run(testTable.Map(o => o.FreeformProperties.ContainsKey("best movie")));
            var numTrue = enumerable.Count(r => r == true);
            var numFalse = enumerable.Count(r => r == false);
            numTrue.Should().Be(1);
            numFalse.Should().Be(2);
        }

        [Test]
        public void Keys()
        {
            var keys = connection.Run(testTable.Filter(o => o.Name == "Madame Curie").Map(o => o.FreeformProperties.Keys)).Single();
            keys.Should().Contain("awesome level");
            keys.Should().Contain("cool level");
            keys.Should().Contain("impressive");
            keys.Should().HaveCount(3);
        }

        [Test]
        public void Values()
        {
            var values = connection.Run(testTable.Filter(o => o.Name == "Madame Curie").Map(o => o.FreeformProperties.Values)).Single();
            values.Should().Contain(15);
            values.Should().Contain(-1);
            values.Should().Contain(true);
            values.Should().HaveCount(3);
        }

        [Test]
        public void ItemGetter()
        {
            var gilGrissomBestKnownFor =
                connection.Run(testTable.Filter(o => o.Name == "Gil Grissom").Map(o => o.FreeformProperties["best known for"])).Single();
            gilGrissomBestKnownFor.Should().Be("CSI: Las Vegas");
        }
    }
}
