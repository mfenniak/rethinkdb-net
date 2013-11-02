using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Configuration;
using RethinkDb.Logging;

namespace RethinkDb.Test.Integration
{
    public class TestBase
    {
        public static IConnectionFactory ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory("testCluster");

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
            connection = await ConnectionFactory.GetAsync();

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

        [TestFixtureTearDown]
        public virtual void TestFixtureTearDown()
        {
            connection.Dispose();
            connection = null;
        }
    }
}
