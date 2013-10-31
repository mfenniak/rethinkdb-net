using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RethinkDb.DatumConverters;
using RethinkDb.Test.Integration;

namespace RethinkDb.Newtonsoft.Test
{
    [TestFixture]
    public class DebugTests
    {
        [Test]
        [Explicit]
        [Description("DEBUG ONLY: Just used to print out what a native datum looks like.")]
        public void debug_print_json_writer()
        {
            var testObject = new TestObject()
                {
                    Id = "MY_ID_HERE",
                    Name = "MY_NAME_HERE",
                    SomeNumber = 123,
                    Tags = new[] {"tag1", "tag2", "tag3"},
                    Children = new[]
                        {
                            new TestObject
                                {
                                    Name = "ArrayC1"
                                }
                        },
                    ChildrenList = new List<TestObject>
                        {
                            new TestObject() {Name = "ListC2"}
                        },
                    ChildrenIList = new List<TestObject>
                        {
                            new TestObject() {Name = "ListC3"}
                        }
                };

            var datum = DatumConvert.SerializeObject(testObject);
            Console.WriteLine(datum.ToDebugString());
        }

        [Test]
        [Explicit]
        [Description("DEBUG ONLY: Just used to print out what a native datum looks like.")]
        public void debug_print_json_reader()
        {
            var objIn = new TestObject()
                {
                    Id = "MY_ID_HERE",
                    Name = "MY_NAME_HERE",
                    SomeNumber = 123,
                    Tags = new[] {"tag1", "tag2", "tag3"},
                    Children = new[]
                        {
                            new TestObject
                                {
                                    Name = "ArrayC1"
                                }
                        },
                    ChildrenList = new List<TestObject>
                        {
                            new TestObject() {Name = "ListC2"}
                        },
                    ChildrenIList = new List<TestObject>
                        {
                            new TestObject() {Name = "ListC3"}
                        }
                };

            var datum = DatumConvert.SerializeObject(objIn);

            Console.WriteLine(datum.ToDebugString());

            var objOut = DatumConvert.DeserializeObject<TestObject>(datum);

            objIn.ShouldBeEquivalentTo(objOut);
        }

        [Test]
        [Explicit]
        [Description("DEBUG ONLY: Just used to print out what a native datum looks like.")]
        public void debug_print_native_datum()
        {
            var testObject = new TestObject()
                {
                    Id = "MY_ID_HERE",
                    Name = "MY_NAME_HERE",
                    SomeNumber = 123,
                    Tags = new[] {"tag1", "tag2", "tag3"},
                    Children = new[]
                        {
                            new TestObject
                                {
                                    Name = "ArrayC1"
                                }
                        },
                    ChildrenList = new List<TestObject>
                        {
                            new TestObject() {Name = "ListC2"}
                        },
                    ChildrenIList = new List<TestObject>
                        {
                            new TestObject() {Name = "ListC3"}
                        }
                };


            var datum = Native.RootFactory.Get<TestObject>().ConvertObject(testObject);

            Console.WriteLine(datum.ToDebugString());
        }
    }
}