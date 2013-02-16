using System;
using System.Runtime.Serialization;

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

                var task1 = connection.Connect("127.0.0.1231");
                task1.Wait();

                var task2 = connection.FetchSingleObject<TestObject>();
                task2.Wait();

                Console.WriteLine("User: {0} ({1})", task2.Result.Name, task2.Result.Email);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);                
            }
        }
    }
}
