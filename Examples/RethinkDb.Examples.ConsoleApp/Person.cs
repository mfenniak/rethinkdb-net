using System;
using System.Runtime.Serialization;

namespace RethinkDb.Examples.ConsoleApp
{
    [DataContract]
    public class Person
    {
        public static IDatabaseQuery Db = Query.Db("test");
        public static ITableQuery<Person> Table = Db.Table<Person>("people");

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public Guid Id;

        [DataMember]
        public string Name;
    }
}

