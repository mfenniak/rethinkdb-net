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

    [TestFixture]
    public class NTableTests : TableTests
    {
        static NTableTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }

    [TestFixture]
    public class NMultiTableTests : MultiTableTests
    {
        static NMultiTableTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }

    [TestFixture]
    public class NMultiObjectTests : MultiObjectTests
    {
        static NMultiObjectTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }

    [TestFixture]
    public class NManyObjectTests : ManyObjectTests
    {
        static NManyObjectTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }

    [TestFixture]
    public class NBlankTests : BlankTests
    {
        static NBlankTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }


    [TestFixture]
    public class NDatabaseTests : DatabaseTests
    {
        static NDatabaseTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }


    [TestFixture]
    public class NGroupingTests : GroupingTests
    {
        static NGroupingTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }


    [TestFixture]
    public class NHasFieldsTests : HasFieldsTests
    {
        static NHasFieldsTests()
        {
            ConnectionFactory = ConfigurationAssembler.CreateConnectionFactory( "testCluster" );
        }
    }
}