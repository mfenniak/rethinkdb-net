using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Configuration;

namespace RethinkDb.Test
{
    public class TestBase
    {
        protected IConnection connection;

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            try
            {
                DoTestFixtureSetUp().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("TestFixtureSetUp failed: {0}", e);
                throw;
            }
        }

        private async Task DoTestFixtureSetUp()
        {
            connection = ConfigConnectionFactory.Instance.Get("testCluster");
            connection.Logger = new DefaultLogger(LoggingCategory.Debug, Console.Out);

            await connection.ConnectAsync();

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

