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
            connection = new Connection();
            await connection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 28015));

            var dbList = await connection.Run(Query.DbList());
            if (dbList.Contains("test"))
            {
                var resp = await connection.Run(Query.DbDrop("test"));
                if (resp.Dropped != 1)
                    throw new Exception("DbDrop failed");
            }
        }
    }
}

