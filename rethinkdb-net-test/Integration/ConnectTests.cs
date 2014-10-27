using System;
using NUnit.Framework;
using RethinkDb;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using RethinkDb.Configuration;
using System.Threading;

namespace RethinkDb.Test.Integration
{
    [TestFixture]
    public class ConnectTests
    {
        [Test]
        [Timeout(10000)]
        [Explicit]
        public void Timeout()
        {
            // Had to pick a routable IP, but one that won't return packets... don't know of a better choice than this
            var connection = new Connection(new IPEndPoint(IPAddress.Parse("10.230.220.210"), 28015));
            connection.ConnectTimeout = TimeSpan.FromSeconds(1);
            connection.Connect();
        }

        [Test]
        public void Cancel()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            try
            {
                var connection = new Connection(new IPEndPoint(IPAddress.Parse("10.230.220.210"), 28015));
                connection.Connect(cts.Token);
                Assert.Fail("Expected exception");
            }
            catch (AggregateException ex)
            {
                Assert.That(ex.InnerException is TaskCanceledException);
            }
        }
    }
}

