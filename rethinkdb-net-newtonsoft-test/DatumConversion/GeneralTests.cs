using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using RethinkDb.DatumConverters;
using RethinkDb.Newtonsoft.Test.TestObjects;
using RethinkDb.Test.Integration;

namespace RethinkDb.Newtonsoft.Test.DatumConversion
{
    [TestFixture]
    public class GeneralTests
    {
        [Test]
        //Quick test to show the equivalence of Newton's serialization and the native driver's DataContract serialization.
        public void native_and_newton_basic_comparison_test1()
        {
            var testObject = TestObjectWithTestData();

            var nativeDatum = Native.RootFactory.Get<TestObject>().ConvertObject(testObject);

            var newtonDatum = DatumConvert.SerializeObject(testObject);

            newtonDatum.ShouldBeEquivalentTo(nativeDatum);
        }

        [Test]
        // TestObject and TestObjectNewton are the same, but shows the difference between the attributing style and elegance.
        public void similar_object_models_with_different_attributing_should_be_equivalent()
        {
            var withAttributes = TestObjectWithTestData();

            var withoutAttributes = TestObjectNewtonWithTestData();

            var datumFromContractAttributes = Native.RootFactory.Get<TestObject>().ConvertObject(withAttributes);

            var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

            var datumFromNewton = DatumConvert.SerializeObject(withoutAttributes, jsonSettings);

            datumFromContractAttributes.ShouldBeEquivalentTo(datumFromNewton);
        }

        [Test]
        //Converting an object to and from a datum should be the same.
        public void newton_datum_regurgitation_should_be_equivlant()
        {
            var objIn = TestObjectWithTestData();

            var newtonDatum = DatumConvert.SerializeObject(objIn);

            var objOut = DatumConvert.DeserializeObject<TestObject>(newtonDatum);

            var newtonDatum2 = DatumConvert.SerializeObject(objOut);

            newtonDatum.ShouldBeEquivalentTo(newtonDatum2);
            newtonDatum.r_object.Count.Should().Be(8); // sanity check
        }

        public TestObject TestObjectWithTestData()
        {
            var obj = new TestObject();
            obj.Id = "my_id_1234";
            obj.Name = "Brian Chavez";
            obj.SomeNumber = 1234.5;
            obj.Tags = new[] {"tag1", "tag2", "tag3"};

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

        public TestObjectNewton TestObjectNewtonWithTestData()
        {
            var obj = new TestObjectNewton();
            obj.Id = "my_id_1234";
            obj.Name = "Brian Chavez";
            obj.SomeNumber = 1234.5;
            obj.Tags = new[] {"tag1", "tag2", "tag3"};

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