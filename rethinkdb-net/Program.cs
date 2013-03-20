using System;
using System.Runtime.Serialization;
using System.Net;

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
                var connection = new Connection();

                var task1 = connection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28015));
                task1.Wait();

                var query2 = Query.Db("voicemail").Table("user");
                var task2 = connection.FetchSingleObject<TestObject>(query2);
                task2.Wait();
                Console.WriteLine("User: {0} ({1})", task2.Result.Name, task2.Result.Email);

                var query3 = Query.Db("voicemail").Table("user").Get("58379951-6208-46cc-a194-03da8ee1e13c");
                var task3 = connection.FetchSingleObject<TestObject>(query3);
                task3.Wait();
                Console.WriteLine("User: {0}", task3.Result);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
        }
    }
}
