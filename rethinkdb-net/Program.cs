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
            var connection = new Connection();

            var task = connection.Connect("10.210.27.106");
            task.Wait();

            var dcs = new RethinkDb.DataContractJsonSerializer<TestObject>();

            task = connection.FetchSingleObject<TestObject>(dcs);
            task.Wait();
        }
    }
}
