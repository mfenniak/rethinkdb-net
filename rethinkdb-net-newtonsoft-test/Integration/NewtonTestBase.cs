using NUnit.Framework;
using RethinkDb.Newtonsoft.Configuration;
using RethinkDb.Test.Integration;

namespace RethinkDb.Newtonsoft.Test.Integration
{
    [TestFixture]
    public class NSingleObjectTest : SingleObjectTests
    {
        static NSingleObjectTest()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }
}