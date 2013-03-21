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
                if (resp.Created != 1)
                    throw new Exception("DbCreate failed");

                var testDb = Query.Db("test");

                resp = await connection.Run(testDb.TableCreate("table"));
                if (resp.Created != 1)
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
                if (resp.Inserted != 1)
                    throw new Exception("Insert failed");
                else if (resp.GeneratedKeys == null || resp.GeneratedKeys.Length != 1)
                    throw new Exception("Insert failed; GeneratedKeys bad");

                obj = await connection.Run(testTable.Get(resp.GeneratedKeys[0]));
                if (obj == null)
                    throw new Exception("Get failed from FetchSingleObject");

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
                if (count != 1501)
                    throw new Exception("Table query found unexpected objects");

                resp = await connection.Run(testDb.TableDrop("table"));
                if (resp.Dropped != 1)
                    throw new Exception("TableDrop failed");

                resp = await connection.Run(Query.DbDrop("test"));
                if (resp.Dropped != 1)
                    throw new Exception("DbDrop failed");
            }
        }
    }
}
