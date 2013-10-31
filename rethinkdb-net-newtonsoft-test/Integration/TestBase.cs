using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using RethinkDb.Configuration;
using RethinkDb.ConnectionFactories;

namespace RethinkDb.Newtonsoft.Test.Integration
{
    public class TestBase
    {
        private static IConnectionFactory connectionFactory;

        protected IConnection connection;

        static TestBase()
        {
            ConfigurationAssembler.ConnectionFactoryProvider = list => new NewtonsoftConnectionFactory(list);

            connectionFactory = ConfigurationAssembler.CreateConnectionFactory("testCluster");
        }

        public class NewtonsoftConnectionFactory : DefaultConnectionFactory
        {
            public NewtonsoftConnectionFactory(List<EndPoint> endPoints) : base(endPoints)
            {
            }
            public override async Task<IConnection> GetAsync()
            {
                var conn = await base.GetAsync();
                conn.DatumConverterFactory = new NewtonSerializer();
                return conn;
            }
        }

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            try
            {
                DoTestFixtureSetUp().Wait();
            }
            catch( Exception e )
            {
                Console.WriteLine( "TestFixtureSetUp failed: {0}", e );
                throw;
            }
        }

        private async Task DoTestFixtureSetUp()
        {
            connection = await connectionFactory.GetAsync();

            try
            {
                var dbList = await connection.RunAsync( Query.DbList() );
                if( dbList.Contains( "test" ) )
                    await connection.RunAsync( Query.DbDrop( "test" ) );
            }
            catch( Exception )
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