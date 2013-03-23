using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RethinkDb.Test
{
    [DataContract]
    public class TestObject
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "children")]
        public TestObject[] Children;
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var task = TestSequence();
                task.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
        }

        private static async Task TestSequence()
        {
            using (var connection = new Connection())
            {
                DmlResponse resp;

                await connection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28015));

                var dbList = await connection.Run(Query.DbList());
                if (dbList.Contains("test"))
                {
                    resp = await connection.Run(Query.DbDrop("test"));
                    if (resp.Dropped != 1)
                        throw new Exception("DbDrop failed");
                }

                resp = await connection.Run(Query.DbCreate("test"));
                if (resp.Created != 1 || resp.FirstError != null)
                    throw new Exception("DbCreate failed");

                var testDb = Query.Db("test");

                resp = await connection.Run(testDb.TableCreate("table"));
                if (resp.Created != 1 || resp.FirstError != null)
                    throw new Exception("TableCreate failed");

                var tableList = await connection.Run(testDb.TableList());
                if (tableList.Length != 1 || tableList[0] != "table")
                    throw new Exception("TableList failed");

                var testTable = testDb.Table<TestObject>("table");

                var obj = await connection.Run(testTable.Get("58379951-6208-46cc-a194-03da8ee1e13c"));
                if (obj != null)
                    throw new Exception("Expected null from fetching a random GUID");

                var enumerable = connection.Run(testTable);
                int count = 0;
                while (true)
                {
                    if (!await enumerable.MoveNext())
                        break;
                    ++count;
                }
                if (count != 0)
                    throw new Exception("Table query found unexpected objects");

                obj = new TestObject()
                {
                    Name = "Jim Brown",
                    Children = new TestObject[] {
                        new TestObject() { Name = "Scan" }
                    }
                };
                resp = await connection.Run(testTable.Insert(obj));
                if (resp.Inserted != 1 || resp.FirstError != null)
                    throw new Exception("Insert failed");
                else if (resp.GeneratedKeys == null || resp.GeneratedKeys.Length != 1)
                    throw new Exception("Insert failed; GeneratedKeys bad");

                var objKey = resp.GeneratedKeys[0];

                obj = await connection.Run(testTable.Get(objKey));
                if (obj == null)
                    throw new Exception("Get failed from FetchSingleObject");

                resp = await connection.Run(testTable.Get(objKey).Replace(new TestObject() { Id = objKey, Name = "Jack Black" }));
                if (resp.Replaced != 1 || resp.FirstError != null)
                    throw new Exception("Replace failed");

                resp = await connection.Run(testTable.Get(objKey).Delete());
                if (resp.Deleted != 1 || resp.FirstError != null)
                    throw new Exception("Delete failed");

                resp = await connection.Run(testTable.Delete());
                if (resp.Deleted != 0 || resp.FirstError != null)
                    throw new Exception("Delete failed");

                resp = await connection.Run(testTable.Insert(new TestObject[] {
                    new TestObject() { Id = "1", Name = "1" },
                    new TestObject() { Id = "2", Name = "2" },
                    new TestObject() { Id = "3", Name = "3" },
                    new TestObject() { Id = "4", Name = "4" },
                    new TestObject() { Id = "5", Name = "5" },
                    new TestObject() { Id = "6", Name = "6" },
                    new TestObject() { Id = "7", Name = "7" },
                }));
                if (resp.Inserted != 7  || resp.FirstError != null)
                    throw new Exception("Insert failed");

                enumerable = connection.Run(testTable.Between("2", "4"));
                count = 0;
                while (true)
                {
                    if (!await enumerable.MoveNext())
                        break;
                    ++count;
                }
                if (count != 3)
                    throw new Exception("Table query found unexpected objects");

                enumerable = connection.Run(testTable.Between(null, "4"));
                count = 0;
                while (true)
                {
                    if (!await enumerable.MoveNext())
                        break;
                    ++count;
                }
                if (count != 4)
                    throw new Exception("Table query found unexpected objects");

                resp = await connection.Run(testTable.Between(null, "4").Delete());
                if (resp.Deleted != 4 || resp.FirstError != null)
                    throw new Exception("Delete failed");

                resp = await connection.Run(testTable.Delete());
                if (resp.Deleted != 3 || resp.FirstError != null)
                    throw new Exception("Delete failed");

                // Insert more than 1000 objects to test the enumerable loading additional chunks of the sequence
                var objectList = new List<TestObject>();
                for (int i = 0; i < 1500; i++)
                    objectList.Add(new TestObject() { Name = "Object #" + i });
                resp = await connection.Run(testTable.Insert(objectList));
                if (resp.Inserted != 1500)
                    throw new Exception("Insert failed");

                enumerable = connection.Run(testTable);
                count = 0;
                while (true)
                {
                    if (!await enumerable.MoveNext())
                        break;
                    ++count;
                }
                if (count != 1500)
                    throw new Exception("Table query found unexpected objects");

                resp = await connection.Run(testTable.Delete());
                if (resp.Deleted != 1500 || resp.FirstError != null)
                    throw new Exception("Delete failed");


                resp = await connection.Run(testTable.Insert(new TestObject[] {
                    new TestObject() { Id = "1", Name = "1" },
                    new TestObject() { Id = "2", Name = "2" },
                    new TestObject() { Id = "3", Name = "3" },
                    new TestObject() { Id = "4", Name = "4" },
                    new TestObject() { Id = "5", Name = "5" },
                    new TestObject() { Id = "6", Name = "6" },
                    new TestObject() { Id = "7", Name = "7" },
                }));
                if (resp.Inserted != 7  || resp.FirstError != null)
                    throw new Exception("Insert failed");

                resp = await connection.Run(testTable.Update(obj => new TestObject() { Name = "Hello!" }));
                if (resp.Replaced != 7 || resp.FirstError != null) // "Replaced" seems weird here, but that's what RethinkDB returns in Data Explorer too
                    throw new Exception("Update failed");

                resp = await connection.Run(testTable.Update(obj => new TestObject() { Name = "Hello " + obj.Id + "!" }));
                if (resp.Replaced != 7 || resp.FirstError != null) // "Replaced" seems weird here, but that's what RethinkDB returns in Data Explorer too
                    throw new Exception("Update failed");


                resp = await connection.Run(testDb.TableDrop("table"));
                if (resp.Dropped != 1 || resp.FirstError != null)
                    throw new Exception("TableDrop failed");

                resp = await connection.Run(Query.DbDrop("test"));
                if (resp.Dropped != 1 || resp.FirstError != null)
                    throw new Exception("DbDrop failed");
            }
        }
    }
}
