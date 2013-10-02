using System;
using System.Linq;
using RethinkDb;
using RethinkDb.Configuration;

namespace RethinkDb.Examples.ConsoleApp
{
    public static class Program
    {
        private static IConnectionFactory connectionFactory = ConfigurationAssembler.CreateConnectionFactory("example");

        public static void Main(string[] args)
        {
            var conn = connectionFactory.Get();

            // Create DB if needed
            if (!conn.Run(Query.DbList()).Contains("test"))
                conn.Run(Query.DbCreate("test"));

            // Create table if needed
            if (!conn.Run(Person.Db.TableList()).Contains("people"))
                conn.Run(Person.Db.TableCreate("people"));

            // Read all the contents of the table
            foreach (var person in conn.Run(Person.Table))
                Console.WriteLine("Id: {0}, Name: {1}", person.Id, person.Name);

            // Insert a new record
            conn.Run(Person.Table.Insert(new Person() { Name = "Jack Black" }));
        }
    }
}
