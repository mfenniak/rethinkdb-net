using System;
using NUnit.Framework;
using RethinkDb;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    [Ignore]
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
                            },
                            IntegerProperties = new Dictionary<string, int>()
                            {
                                { "awesome level", 100 },
                                { "cool level", 15 },
                            },
                            StringProperties = new Dictionary<string, string>()
                            {
                                { "best movie", "School of Rock" },
                                { "oscar winning movie", null }
                            },
                        },
                        new TestObjectWithDictionary()
                        {
                            Name = "Gil Grissom",
                            FreeformProperties = new Dictionary<string, object>()
                            {
                                { "awesome level", 101 },
                                { "cool level", 0 },
                                { "best known for", "CSI: Las Vegas" }
                            },
                            IntegerProperties = new Dictionary<string, int>()
                            {
                                { "awesome level", 101 },
                                { "cool level", 0 },
                            },
                        },
                        new TestObjectWithDictionary()
                        {
                            Name = "Madame Curie",
                            FreeformProperties = new Dictionary<string, object>()
                            {
                                { "awesome level", 15 },
                                { "cool level", -1 },
                                { "impressive", true }
                            },
                            IntegerProperties = new Dictionary<string, int>()
                            {
                                { "awesome level", 15 },
                                { "cool level", -1 },
                            },
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
        public void ContainsKeyTyped()
        {
            var enumerable = connection.Run(testTable.Map(o => o.StringProperties != null && o.StringProperties.ContainsKey("best movie")));
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
        public void KeysTypes()
        {
            var keys = connection.Run(testTable.Filter(o => o.Name == "Madame Curie").Map(o => o.IntegerProperties.Keys)).Single();
            keys.Should().Contain("awesome level");
            keys.Should().Contain("cool level");
            keys.Should().HaveCount(2);
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
        public void ValuesTyped()
        {
            var values = connection.Run(testTable.Filter(o => o.Name == "Madame Curie").Map(o => o.IntegerProperties.Values)).Single();
            values.Should().Contain(15);
            values.Should().Contain(-1);
            values.Should().HaveCount(2);
        }

        [Test]
        public void ItemGetter()
        {
            var gilGrissomBestKnownFor =
                connection.Run(testTable.Filter(o => o.Name == "Gil Grissom").Map(o => o.FreeformProperties["best known for"])).Single();
            gilGrissomBestKnownFor.Should().Be("CSI: Las Vegas");
        }

        [Test]
        public void ItemGetterTyped()
        {
            var gilGrissomBestKnownFor =
                connection.Run(testTable.Filter(o => o.Name == "Jack Black").Map(o => o.StringProperties["best movie"])).Single();
            gilGrissomBestKnownFor.Should().Be("School of Rock");
        }

        [Test]
        public void ItemSetter()
        {
            var gilGrissomBestKnownFor = connection.Run(
                testTable
                    .Filter(o => o.Name == "Gil Grissom")
                    .Update(o => new TestObjectWithDictionary() { FreeformProperties = o.FreeformProperties.SetValue("best known for", "being awesome") })
            );

            gilGrissomBestKnownFor.Replaced.Should().Be(1);
            var gil = connection.Run(testTable.Filter(o => o.Name == "Gil Grissom")).Single();
            gil.FreeformProperties.Should().Contain("best known for", "being awesome");
            gil.FreeformProperties.Should().HaveCount(3);
        }

        [Test]
        public void ItemSetterTyped()
        {
            var gilGrissomBestKnownFor = connection.Run(
                testTable
                .Filter(o => o.Name == "Jack Black")
                .Update(o => new TestObjectWithDictionary() { StringProperties = o.StringProperties.SetValue("best movie", "High Fidelity") })
            );

            gilGrissomBestKnownFor.Replaced.Should().Be(1);
            var gil = connection.Run(testTable.Filter(o => o.Name == "Jack Black")).Single();
            gil.StringProperties.Should().Contain("best movie", "High Fidelity");
            gil.StringProperties.Should().HaveCount(2);
        }

        [Test]
        public void MultipleItemSetter()
        {
            var gilGrissomBestKnownFor = connection.Run(
                testTable
                .Filter(o => o.Name == "Gil Grissom")
                .Update(o => new TestObjectWithDictionary()
                {
                    FreeformProperties = o.FreeformProperties
                        .SetValue("best known for", "being awesome")
                        .SetValue("skill level", 1000)
                        .SetValue("updated at", DateTimeOffset.UtcNow)
                }
                )
            );

            //var allObjectsAfter = connection.Run(testTable).ToArray();

            gilGrissomBestKnownFor.Replaced.Should().Be(1);
            var gil = connection.Run(testTable.Filter(o => o.Name == "Gil Grissom")).Single();
            MultipleItemSetterVerifyDictionary(gil);
        }

        protected virtual void MultipleItemSetterVerifyDictionary(TestObjectWithDictionary gil)
        {
            gil.FreeformProperties.Should().Contain("best known for", "being awesome");
            gil.FreeformProperties.Should().Contain("skill level", 1000);
            gil.FreeformProperties.Should().ContainKey("updated at");
            gil.FreeformProperties ["updated at"].Should().BeOfType<DateTimeOffset>();
            gil.FreeformProperties.Should().HaveCount(5);
        }

        [Test]
        public void MultipleItemSetterTyped()
        {
            var gilGrissomBestKnownFor = connection.Run(
                testTable
                .Filter(o => o.Name == "Gil Grissom")
                .Update(o => new TestObjectWithDictionary()
                {
                    IntegerProperties = o.IntegerProperties
                        .SetValue("skill level", 100)
                        .SetValue("cunning level", 200)
                        .SetValue("clever level", 300)
                    }
                )
            );

            gilGrissomBestKnownFor.Replaced.Should().Be(1);
            var gil = connection.Run(testTable.Filter(o => o.Name == "Gil Grissom")).Single();
            gil.IntegerProperties.Should().Contain("skill level", 100);
            gil.IntegerProperties.Should().Contain("cunning level", 200);
            gil.IntegerProperties.Should().Contain("clever level", 300);
            gil.IntegerProperties.Should().HaveCount(5);
        }

        [Test]
        public void Without()
        {
            var dict = connection.Run(testTable
                .Filter(o => o.Name == "Madame Curie")
                .Map(o => o.FreeformProperties.Without("awesome level"))).Single();
            dict.Should().ContainKey("cool level");
            dict.Should().ContainKey("impressive");
            dict.Should().HaveCount(2);
        }

        [Test]
        public void FilterByPropertyValue()
        {
            var impressivePeople = connection.Run(testTable.Filter(o => (bool)o.FreeformProperties["impressive"])).ToArray();
            impressivePeople.Should().HaveCount(1);
            impressivePeople [0].Name.Should().Be("Madame Curie");
        }

        [Test]
        public void FilterByTypedPropertyValue()
        {
            var impressivePeople = connection.Run(testTable.Filter(o => o.IntegerProperties["awesome level"] > 100)).ToArray();
            impressivePeople.Should().HaveCount(1);
            impressivePeople [0].Name.Should().Be("Gil Grissom");
        }

        [Test]
        public void FilterByPropertyValueWithOperator()
        {
            var impressivePeople = connection.Run(testTable.Filter(o => (int)o.FreeformProperties["awesome level"] > 100)).ToArray();
            impressivePeople.Should().HaveCount(1);
            impressivePeople [0].Name.Should().Be("Gil Grissom");
        }
    }
}
