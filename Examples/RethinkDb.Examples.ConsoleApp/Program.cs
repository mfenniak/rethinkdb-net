using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RethinkDb;
using RethinkDb.Configuration;

namespace RethinkDb.Examples.ConsoleApp
{
    public static class Program
    {
        private static IConnectionFactory connectionFactory = ConfigurationAssembler.CreateConnectionFactory("example");
        private static CancellationTokenSource stopMonitor = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            Thread changefeedThread;
            using (var conn = connectionFactory.Get())
            {
                // Create DB if needed
                if (!conn.Run(Query.DbList()).Contains("test"))
                    conn.Run(Query.DbCreate("test"));

                // Create table if needed
                if (!conn.Run(Person.Db.TableList()).Contains("people"))
                    conn.Run(Person.Db.TableCreate("people"));

                // Begin monitoring for database changes.
                changefeedThread = new Thread(ChangeFeedMonitor);
                changefeedThread.Start();

                // Read all the contents of the table
                foreach (var person in conn.Run(Person.Table))
                    Console.WriteLine("Id: {0}, Name: {1}", person.Id, person.Name);

                // Insert a new record
                conn.Run(Person.Table.Insert(new Person() { Name = "Jack Black" }));
            }

            Console.WriteLine("Press any key to cancel monitoring.");
            Console.ReadKey();
            stopMonitor.Cancel();

            changefeedThread.Join();
        }

        private static void ChangeFeedMonitor()
        {
            try
            {
                using (var conn = connectionFactory.Get())
                {
                    foreach (var change in conn.Run(Person.Table.Changes(), cancellationToken: stopMonitor.Token))
                    {
                        string type;
                        Guid id;
                        if (change.NewValue == null)
                        {
                            type = "DELETE";
                            id = change.OldValue.Id;
                        }
                        else if (change.OldValue == null)
                        {
                            type = "INSERT";
                            id = change.NewValue.Id;
                        }
                        else
                        {
                            type = "UPDATE";
                            id = change.NewValue.Id;
                        }

                        Console.WriteLine("{0}: Monitored change to Person table, {1} of id {2}", DateTime.Now, type, id);
                    }
                }
            }
            catch (AggregateException ex)
            {
                if (!(ex.InnerException is TaskCanceledException))
                    throw;
            }
        }
    }
}
