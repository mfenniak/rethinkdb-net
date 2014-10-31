using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class TableTests : TestBase
    {
        private ITableQuery<TestObject> testTable;
        private ITableQuery<TestObject2> testTable2;
        private ITableQuery<TestObject3> testTable3;

        [SetUp]
        public virtual void SetUp()
        {
            connection.RunAsync(Query.DbCreate("test")).Wait();
            connection.RunAsync(Query.Db("test").TableCreate("table")).Wait();
            testTable = Query.Db("test").Table<TestObject>("table");
            testTable2 = Query.Db("test").Table<TestObject2>("table");
            testTable3 = Query.Db("test").Table<TestObject3>("table");
        }

        [TearDown]
        public virtual void TearDown()
        {
            connection.RunAsync(Query.DbDrop("test")).Wait();
        }

        [Test]
        public void GetQueryNull()
        {
            DoGetQueryNull().Wait();
        }

        private async Task DoGetQueryNull()
        {
            var obj = await connection.RunAsync(testTable.Get("58379951-6208-46cc-a194-03da8ee1e13c"));
            Assert.That(obj, Is.Null);
        }

        [Test]
        public void EmptyEnumerator()
        {
            DoEmptyEnumerator().Wait();
        }

        private async Task DoEmptyEnumerator()
        {
            var enumerable = connection.RunAsync(testTable);
            int count = 0;
            while (true)
            {
                if (!await enumerable.MoveNext())
                    break;
                ++count;
            }
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void Insert()
        {
            DoInsert().Wait();
        }

        private async Task DoInsert()
        {
            var obj = new TestObject()
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
            };
            var resp = await connection.RunAsync(testTable.Insert(obj));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Inserted, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Not.Null);
            Assert.That(resp.GeneratedKeys, Has.Length.EqualTo(1));
        }

        [Test]
        public void MultiInsert()
        {
            DoMultiInsert().Wait();
        }

        private async Task DoMultiInsert()
        {
            var resp = await connection.RunAsync(testTable.Insert(new TestObject[] {
                new TestObject() { Name = "1" },
                new TestObject() { Name = "2" },
                new TestObject() { Name = "3" },
                new TestObject() { Name = "4" },
                new TestObject() { Name = "5" },
                new TestObject() { Name = "6" },
                new TestObject() { Name = "7" },
            }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Inserted, Is.EqualTo(7));
            Assert.That(resp.GeneratedKeys, Is.Not.Null);
            Assert.That(resp.GeneratedKeys, Has.Length.EqualTo(7));
        }



        [Test]
        public void InsertWithConflictError()
        {
            DoInsertWithConflictError().Wait();
        }

        private async Task DoInsertWithConflictError()
        {
            var guid = new Guid().ToString();

            var resp = await connection.RunAsync(testTable.Insert(
                new TestObject() { Id = guid, Name = "1" }
            ));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.Inserted, Is.EqualTo(1));

            resp = await connection.RunAsync(testTable.Insert(
                new TestObject() { Id = guid, Name = "1" }
            ));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Not.Null.Or.Empty);
            Assert.That(resp.Errors, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }

        [Test]
        public void InsertWithConflictReplace()
        {
            DoInsertWithConflictReplace().Wait();
        }

        private async Task DoInsertWithConflictReplace()
        {
            var guid = new Guid().ToString();

            var resp = await connection.RunAsync(testTable.Insert(
                new TestObject() { Id = guid, Name = "1" }
            ));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.Inserted, Is.EqualTo(1));

            resp = await connection.RunAsync(testTable.Insert(
                new TestObject() { Id = guid, Name = "2" }
            , Conflict.Replace));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.Replaced, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }


        [Test]
        public void InsertWithConflictUpdate()
        {
            DoInsertWithConflictUpdate().Wait();
        }

        private async Task DoInsertWithConflictUpdate()
        {
            var guid = new Guid().ToString();

            var resp = await connection.RunAsync(testTable.Insert(
                new TestObject() { Id = guid, Name = "1" }
            ));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.Inserted, Is.EqualTo(1));

            resp = await connection.RunAsync(testTable.Insert(
                new TestObject() { Id = guid, Name = "2" }
            , Conflict.Update));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.Replaced, Is.EqualTo(1));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }



        [Test]
        public void MultiInsertWithIds()
        {
            DoMultiInsertWithIds().Wait();
        }

        private async Task DoMultiInsertWithIds()
        {
            var resp = await connection.RunAsync(testTable.Insert(new TestObject[] {
                new TestObject() { Id = "1", Name = "1" },
                new TestObject() { Id = "2", Name = "2" },
                new TestObject() { Id = "3", Name = "3" },
                new TestObject() { Id = "4", Name = "4" },
                new TestObject() { Id = "5", Name = "5" },
                new TestObject() { Id = "6", Name = "6" },
                new TestObject() { Id = "7", Name = "7" },
            }));
            Assert.That(resp, Is.Not.Null);
            Assert.That(resp.FirstError, Is.Null);
            Assert.That(resp.Inserted, Is.EqualTo(7));
            Assert.That(resp.GeneratedKeys, Is.Null);
        }

        [Test]
        public void ReduceEmptyTable()
        {
            try
            {
                connection.Run(testTable.Reduce((acc, val) => new TestObject() { SomeNumber = acc.SomeNumber + val.SomeNumber }));
                Assert.Fail("Expected exception");
            }
            catch (AggregateException ex)
            {
                Assert.That(ex.InnerException is RethinkDbRuntimeException);
            }
        }

        [Test]
        public void NumericIdInsert()
        {
            var resp = connection.Run(testTable2.Insert(new TestObject2() { Id = 1, Name = "Woot" }));
            Assert.That(resp.Inserted, Is.EqualTo(1));
            resp = connection.Run(testTable2.Insert(new TestObject2() { Id = 1, Name = "Woot" }));
            Assert.That(resp.Errors, Is.EqualTo(1));
            resp = connection.Run(testTable2.Insert(new TestObject2() { Id = 1, Name = "Woot" }));
            Assert.That(resp.Errors, Is.EqualTo(1));
        }

        [Test]
        public void NumericIdGet()
        {
            var resp = connection.Run(testTable2.Insert(new TestObject2() { Id = 1, Name = "Woot" }));
            Assert.That(resp.Inserted, Is.EqualTo(1));

            var to = connection.Run(testTable2.Get(1));
            Assert.That(to, Is.Not.Null);
            Assert.That(to.Id, Is.EqualTo(1));
        }

        [Test]
        public void LongAndIntTests()
        {
            var testObj = new TestObject3()
            {
                Id = 3,
                Value_Int = 1354353535,
                Value_Int_Nullable = 3,
                Value_Long = 2222222222,
                Value_Long_Nullable = 2
            };

            var resp = connection.Run(testTable3.Insert(testObj));

            Assert.That(resp.Inserted, Is.EqualTo(1));

            var to = connection.Run(testTable3.Get(3));
            Assert.That(to, Is.Not.Null);
            Assert.That(to.Id, Is.EqualTo(3));
        }

        [Test]
        public void IndexCreateSimple()
        {
            var resp = connection.Run(testTable.IndexCreate("index1", o => o.Name));
            Assert.That(resp.Created, Is.EqualTo(1));
        }

        [Test]
        public void IndexCreateExpression()
        {
            var resp = connection.Run(testTable.IndexCreate("index1", o => o.SomeNumber + o.SomeNumber));
            Assert.That(resp.Created, Is.EqualTo(1));
        }

        [Test]
        public void IndexCreateMulti()
        {
            var languages = new[]
                {
                    new TestObject
                        {
                            Name = "C#",
                            Tags = new[] {"dynamic", "lambdas", "object-oriented", "strongly-typed", "generics"}
                        },
                    new TestObject
                        {
                            Name = "java",
                            Tags = new[] {"object-oriented", "strongly-typed", "generics"}
                        },
                    new TestObject
                        {
                            Name = "javascript",
                            Tags = new[] {"dynamic"}
                        }
                };

            var resp = connection.Run(testTable.Insert(languages));
            Assert.That(resp.Inserted, Is.EqualTo(3));

            resp = connection.Run(testTable.IndexCreate("index_tags", x => x.Tags, multiIndex: true));
            Assert.That(resp.Created, Is.EqualTo(1));

            //and query
            var stronglyTyped = connection.Run(testTable.GetAll("strongly-typed", "index_tags"))
                .ToArray();

            Assert.That(stronglyTyped, Is.Not.Null);
            Assert.That(stronglyTyped.Length, Is.EqualTo(2));
            Assert.That(stronglyTyped.All(x => x.Name != languages[2].Name));
        }

        [Test]
        public void IndexCreateMultiObjectModel()
        {
            var languages = new[]
            {
                new TestObject
                {
                    Name = "C#",
                    Tags = new[] {"dynamic", "lambdas", "object-oriented", "strongly-typed", "generics"}
                },
                new TestObject
                {
                    Name = "java",
                    Tags = new[] {"object-oriented", "strongly-typed", "generics"}
                },
                new TestObject
                {
                    Name = "javascript",
                    Tags = new[] {"dynamic"}
                }
            };

            var resp = connection.Run(testTable.Insert(languages));
            Assert.That(resp.Inserted, Is.EqualTo(3));

            var index = testTable.IndexDefineMulti("index_tags", x => x.Tags);
            resp = connection.Run(index.IndexCreate());
            Assert.That(resp.Created, Is.EqualTo(1));

            //and query
            var stronglyTyped = connection.Run(index.GetAll("strongly-typed")).ToArray();

            Assert.That(stronglyTyped, Is.Not.Null);
            Assert.That(stronglyTyped.Length, Is.EqualTo(2));
            Assert.That(stronglyTyped.All(x => x.Name != languages[2].Name));
        }

        [Test]
        public void IndexList()
        {
            IndexCreateSimple();
            string[] indexes = connection.Run(testTable.IndexList()).ToArray();
            Assert.That(indexes.Length, Is.EqualTo(1));
            Assert.That(indexes[0], Is.EqualTo("index1"));
        }

        [Test]
        public void IndexDrop()
        {
            IndexCreateSimple();
            var resp = connection.Run(testTable.IndexDrop("index1"));
            Assert.That(resp.Dropped, Is.EqualTo(1));
        }

        [Test]
        public void DateTimeOffsetDateTimeConstructor()
        {
            var testObj = new TestObject3()
            {
                Value_Int = 8,
                Value_Long = 2,
            };
            var resp = connection.Run(testTable3.Insert(testObj));
            Assert.That(resp.Inserted, Is.EqualTo(1));

            var result = connection.Run(testTable3.Map(testObject => new {
                DTO = new DateTimeOffset(new DateTime(2014, 6, 6, 6, 6, testObject.Value_Int)),
            })).Single();
            // this test is questionable because DateTimeOffset would normally be local in .NET, but we can't support
            // that server-side because we don't know the client-side's timezone in the expression tree.
            Assert.That(result.DTO, Is.EqualTo(new DateTimeOffset(new DateTime(2014, 6, 6, 6, 6, 8), TimeSpan.Zero)));
        }

        [Test]
        public void DateTimeOffsetDateTimeTimeSpanConstructor()
        {
            var testObj = new TestObject3()
            {
                Value_Int = 8,
                Value_Long = 2,
            };
            var resp = connection.Run(testTable3.Insert(testObj));
            Assert.That(resp.Inserted, Is.EqualTo(1));

            var result = connection.Run(testTable3.Map(testObject => new {
                DTO = new DateTimeOffset(new DateTime(2014, 6, 6, 6, 6, 6), TimeSpan.FromHours(testObj.Value_Int)),
            })).Single();
            Assert.That(result.DTO, Is.EqualTo(new DateTimeOffset(new DateTime(2014, 6, 6, 6, 6, 6), TimeSpan.FromHours(8))));
        }

        [Test]
        public void DateTimeOffsetTicksTimeSpanConstructor()
        {
            var value = new DateTimeOffset(2014, 1, 2, 3, 4, 5, 6, TimeSpan.Zero);
            var testObj = new TestObject3()
            {
                Value_Int = 8,
                Value_Long = value.UtcTicks,
            };
            var resp = connection.Run(testTable3.Insert(testObj));
            Assert.That(resp.Inserted, Is.EqualTo(1));

            var result = connection.Run(testTable3.Map(testObject => new {
                DTO = new DateTimeOffset(testObject.Value_Long, TimeSpan.FromHours(-2.5))
            })).Single();
            Assert.That(result.DTO, Is.EqualTo(new DateTimeOffset(testObj.Value_Long, TimeSpan.FromHours(-2.5))));
        }

        [Test]
        public void DateTimeOffsetYMDHMSTimeSpanConstructor()
        {
            var testObj = new TestObject3()
            {
                Value_Int = 8,
                Value_Long = 2,
            };
            var resp = connection.Run(testTable3.Insert(testObj));
            Assert.That(resp.Inserted, Is.EqualTo(1));

            var result = connection.Run(testTable3.Map(testObject => new {
                DTO = new DateTimeOffset(2014, 1, 2, testObject.Value_Int, 4, 5, TimeSpan.FromHours(-2.5)),
            })).Single();
            Assert.That(result.DTO, Is.EqualTo(new DateTimeOffset(2014, 1, 2, 8, 4, 5, TimeSpan.FromHours(-2.5))));
        }

        [Test]
        public void DateTimeOffsetYMDHMSmsTimeSpanConstructor()
        {
            var testObj = new TestObject3()
            {
                Value_Int = 9,
                Value_Long = 2,
            };
            var resp = connection.Run(testTable3.Insert(testObj));
            Assert.That(resp.Inserted, Is.EqualTo(1));

            var result = connection.Run(testTable3.Map(testObject => new {
                DTO = new DateTimeOffset(2014, 1, 2, 3, 4, 5, testObject.Value_Int, TimeSpan.FromHours(-2.5)),
            })).Single();
            Assert.That(result.DTO, Is.EqualTo(new DateTimeOffset(2014, 1, 2, 3, 4, 5, 9, TimeSpan.FromHours(-2.5))));
        }

        [Test]
        public void Changes()
        {
            DoChanges().Wait();
        }

        private async Task DoChanges()
        {
            var enumerator = connection.RunAsync(testTable.Changes());
            try
            {
                var moveNext = enumerator.MoveNext();
                using (var secondConnection = ConnectionFactory.Get())
                {
                    await secondConnection.RunAsync(testTable.Insert(new TestObject() { Name = "Jim Brown" }));
                }

                Assert.That(await moveNext, Is.True);
                var change = enumerator.Current;
                Assert.That(change.OldValue, Is.Null);
                Assert.That(change.NewValue, Is.Not.Null);
                Assert.That(change.NewValue.Name, Is.EqualTo("Jim Brown"));
            }
            finally
            {
                Console.WriteLine("Disposing iterator...");
                enumerator.Dispose().Wait();
                Console.WriteLine("Disposed!");
            }
        }
    }
}
