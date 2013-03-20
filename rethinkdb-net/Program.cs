using System;
using System.Runtime.Serialization;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace RethinkDb
{
    [DataContract]
    class TestObject
    {
        [DataMember(Name = "openid")]
        public Uri OpenId;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "email")]
        public string Email;

        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name = "phone_number")]
        public string PhoneNumber;

        [DataMember(Name = "phone_number_friendly")]
        public string PhoneNumberFriendly;

        [DataMember(Name = "customer_id")]
        public string CustomerId;

        [DataMember(Name = "phone_number_sid")]
        public string PhoneNumberSid;
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

                var dbList = await connection.Query<string[]>(Query.DbList());
                if (dbList.Contains("test"))
                {
                    resp = await connection.Write(Query.DbDrop("test"));
                    if (resp.Dropped != 1)
                        throw new Exception("DbDrop failed");
                }

                resp = await connection.Write(Query.DbCreate("test"));
                if (resp.Created != 1)
                    throw new Exception("DbCreate failed");

                var testDb = Query.Db("test");

                resp = await connection.Write(testDb.TableCreate("table"));
                if (resp.Created != 1)
                    throw new Exception("TableCreate failed");

                var testTable = testDb.Table("table");

                var obj = await connection.Query<TestObject>(testTable.Get("58379951-6208-46cc-a194-03da8ee1e13c"));
                if (obj != null)
                    throw new Exception("Expected null from fetching a random GUID");

                var enumerable = connection.Query<TestObject>(testTable);
                int count = 0;
                while (true)
                {
                    if (!await enumerable.MoveNext())
                        break;
                    ++count;
                }
                if (count != 0)
                    throw new Exception("Table query found unexpected objects");

                resp = await connection.Write(testTable.Insert<TestObject>(new TestObject() { Name = "Jim Brown" }));
                if (resp.Inserted != 1)
                    throw new Exception("Insert failed");
                else if (resp.GeneratedKeys == null || resp.GeneratedKeys.Length != 1)
                    throw new Exception("Insert failed; GeneratedKeys bad");

                obj = await connection.Query<TestObject>(testTable.Get(resp.GeneratedKeys[0]));
                if (obj == null)
                    throw new Exception("Get failed from FetchSingleObject");

                resp = await connection.Write(testDb.TableDrop("table"));
                if (resp.Dropped != 1)
                    throw new Exception("TableDrop failed");

                resp = await connection.Write(Query.DbDrop("test"));
                if (resp.Dropped != 1)
                    throw new Exception("DbDrop failed");


                /*
                var query2 = Query.Db("voicemail").Table("user");
                var task2 = connection.FetchSingleObject<TestObject>(query2);
                task2.Wait();
                Console.WriteLine("User: {0} ({1})", task2.Result.Name, task2.Result.Email);

                var query3 = Query.Db("voicemail").Table("user").Get("58379951-6208-46cc-a194-03da8ee1e13c");
                var task3 = connection.FetchSingleObject<TestObject>(query3);
                task3.Wait();
                Console.WriteLine("User: {0}", task3.Result);
                */
            }
        }
    }
}
