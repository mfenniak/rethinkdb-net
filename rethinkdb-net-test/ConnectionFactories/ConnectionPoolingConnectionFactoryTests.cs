using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using RethinkDb.ConnectionFactories;
using System.Threading;

namespace RethinkDb.Test.ConnectionFactories
{
    [TestFixture]
    public class ConnectionPoolingConnectionFactoryTests
    {
        private IConnectionFactory rootConnectionFactory;

        [SetUp]
        public void SetUp()
        {
            var realConnection1 = Substitute.For<IConnection>();
            realConnection1.RunAsync(Arg.Any<IDatumConverterFactory>(), Arg.Any<IExpressionConverterFactory>(), (ISingleObjectQuery<int>)null, Arg.Any<CancellationToken>()).Returns(
                y => { var x = new TaskCompletionSource<int>(); x.SetResult(1); return x.Task; }
            );

            var realConnection2 = Substitute.For<IConnection>();
            realConnection2.RunAsync(Arg.Any<IDatumConverterFactory>(), Arg.Any<IExpressionConverterFactory>(), (ISingleObjectQuery<int>)null, Arg.Any<CancellationToken>()).Returns(
                y => { var x = new TaskCompletionSource<int>(); x.SetResult(2); return x.Task; }
            );

            rootConnectionFactory = Substitute.For<IConnectionFactory>();
            rootConnectionFactory.GetAsync().Returns<Task<IConnection>>(
                y => { var x = new TaskCompletionSource<IConnection>(); x.SetResult(realConnection1); return x.Task; },
                y => { var x = new TaskCompletionSource<IConnection>(); x.SetResult(realConnection2); return x.Task; }
            );
        }

        private void AssertRealConnection1(IConnection conn)
        {
            Assert.That(conn, Is.Not.Null);
            Assert.That(conn.Run((ISingleObjectQuery<int>)null), Is.EqualTo(1));
        }

        private void AssertRealConnection2(IConnection conn)
        {
            Assert.That(conn, Is.Not.Null);
            Assert.That(conn.Run((ISingleObjectQuery<int>)null), Is.EqualTo(2));
        }

        [Test]
        public void RetrieveSameConnectionOverAndOver()
        {
            var cf = new ConnectionPoolingConnectionFactory(rootConnectionFactory);

            var conn1 = cf.Get();
            AssertRealConnection1(conn1);
            conn1.Dispose();

            conn1 = cf.Get();
            AssertRealConnection1(conn1);
            conn1.Dispose();

            conn1 = cf.Get();
            AssertRealConnection1(conn1);
            conn1.Dispose();

            conn1 = cf.Get();
            AssertRealConnection1(conn1);
            conn1.Dispose();
        }

        [Test]
        public void SecondConnectionEstablishedIfFirstInUse()
        {
            var cf = new ConnectionPoolingConnectionFactory(rootConnectionFactory);
            var conn1 = cf.Get();
            var conn2 = cf.Get();
            AssertRealConnection1(conn1);
            AssertRealConnection2(conn2);
            conn1.Dispose();
            conn2.Dispose();
        }
    }
}

