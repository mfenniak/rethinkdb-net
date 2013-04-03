using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

namespace RethinkDb.Test
{
    public class TestBase
    {
        protected Connection connection;

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            DoTestFixtureSetUp().Wait();
        }

        private async Task DoTestFixtureSetUp()
        {
            connection = new Connection
            {
                ConnectTimeout = TimeSpan.FromSeconds(30),
                QueryTimeout = TimeSpan.FromSeconds(30)
            };

            await connection.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28015));

            try
            {
                var dbList = await connection.RunAsync(Query.DbList());
                if (dbList.Contains("test"))
                    await connection.RunAsync(Query.DbDrop("test"));
            }
            catch (Exception)
            {
            }
        }
    }
}

