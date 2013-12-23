using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using RethinkDb.ConnectionFactories;
using System.Threading;

namespace RethinkDb.Test.ConnectionFactories
{
    [TestFixture]
    public class ReliableConnectionFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        private IConnectionFactory CreateRootConnectionFactory(IConnection connection)
        {
            var rootConnectionFactory = Substitute.For<IConnectionFactory>();
            rootConnectionFactory.GetAsync().Returns<Task<IConnection>>(
                y => { var x = new TaskCompletionSource<IConnection>(); x.SetResult(connection); return x.Task; }
            );
            return rootConnectionFactory;
        }

        private IConnectionFactory CreateRootConnectionFactory(IConnection conn1, IConnection conn2)
        {
            var rootConnectionFactory = Substitute.For<IConnectionFactory>();
            rootConnectionFactory.GetAsync().Returns<Task<IConnection>>(
                y => { var x = new TaskCompletionSource<IConnection>(); x.SetResult(conn1); return x.Task; },
                y => { var x = new TaskCompletionSource<IConnection>(); x.SetResult(conn2); return x.Task; }
            );
            return rootConnectionFactory;
        }

        [Test]
        public void GetCallsUnderlyingFactory()
        {
            var rootConnectionFactory = Substitute.For<IConnectionFactory>();
            var cf = new ReliableConnectionFactory(rootConnectionFactory);

            cf.Get();

            rootConnectionFactory.Received().GetAsync();
        }

        [Test]
        public void DelegateRunISingleObjectQuery()
        {
            var mockConnection = Substitute.For<IConnection>();
            mockConnection.RunAsync(Arg.Any<IDatumConverterFactory>(), Arg.Any<IExpressionConverterFactory>(), (ISingleObjectQuery<int>)null, Arg.Any<CancellationToken>()).Returns(
                y => { var x = new TaskCompletionSource<int>(); x.SetResult(1); return x.Task; }
            );

            var rootConnectionFactory = CreateRootConnectionFactory(mockConnection);
            var cf = new ReliableConnectionFactory(rootConnectionFactory);

            var conn = cf.Get();
            Assert.That(conn, Is.Not.Null);
            Assert.That(conn.Run((ISingleObjectQuery<int>)null), Is.EqualTo(1));
            conn.Dispose();
        }

        [Test]
        public void RetryRunISingleObjectQuery()
        {
            var errorConnection = Substitute.For<IConnection>();
            errorConnection
                .RunAsync(Arg.Any<IDatumConverterFactory>(), Arg.Any<IExpressionConverterFactory>(), (ISingleObjectQuery<int>)null, Arg.Any<CancellationToken>())
                .Returns(x => { throw new RethinkDbNetworkException("!"); });

            var successConnection = Substitute.For<IConnection>();
            successConnection
                .RunAsync(Arg.Any<IDatumConverterFactory>(), Arg.Any<IExpressionConverterFactory>(), (ISingleObjectQuery<int>)null, Arg.Any<CancellationToken>())
                .Returns(y => { var x = new TaskCompletionSource<int>(); x.SetResult(1); return x.Task; });

            var rootConnectionFactory = CreateRootConnectionFactory(errorConnection, successConnection);
            var cf = new ReliableConnectionFactory(rootConnectionFactory);

            var conn = cf.Get();
            Assert.That(conn, Is.Not.Null);
            Assert.That(conn.Run((ISingleObjectQuery<int>)null), Is.EqualTo(1));
            conn.Dispose();

            // Error connection was attempted...
            errorConnection.Received().RunAsync(Arg.Any<IDatumConverterFactory>(), Arg.Any<IExpressionConverterFactory>(), (ISingleObjectQuery<int>)null, Arg.Any<CancellationToken>());
            // Then another connection was attempted after the error failed.
            successConnection.Received().RunAsync(Arg.Any<IDatumConverterFactory>(), Arg.Any<IExpressionConverterFactory>(), (ISingleObjectQuery<int>)null, Arg.Any<CancellationToken>());
        }
    }
}

