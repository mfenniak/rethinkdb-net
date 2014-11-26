using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RethinkDb.ConnectionFactories
{
    /// <summary>
    /// Very simple connection pool implementation of IConnectionFactory; when connections are Unget into the pool
    /// they're stored in a linked list (for O(1) add/remove operations).
    /// </summary>
    public class ConnectionPoolingConnectionFactory : IConnectionFactory
    {
        private IConnectionFactory innerConnectionFactory;
        private LinkedList<IConnection> pool = new LinkedList<IConnection>();
        private TimeSpan _queryTimeout = new TimeSpan(0, 0, 30) ;

        public ConnectionPoolingConnectionFactory(IConnectionFactory innerConnectionFactory)
        {
            this.innerConnectionFactory = innerConnectionFactory;
        }

        public ConnectionPoolingConnectionFactory(IConnectionFactory innerConnectionFactory, TimeSpan queryTimeout)
        {
            this.innerConnectionFactory = innerConnectionFactory;
            this._queryTimeout = queryTimeout;
        }

        public async Task<IConnection> GetAsync()
        {
            lock (pool)
            {
                var node = pool.First;
                if (node != null)
                {
                    pool.Remove(node);
                    node.Value.QueryTimeout = _queryTimeout;
                    return new PooledConnectionWrapper(this, node.Value);
                }
            }

            // Couldn't get a connection from the pool, so create a new one.
            var connection = await innerConnectionFactory.GetAsync();
            connection.QueryTimeout = _queryTimeout;
            return new PooledConnectionWrapper(this, connection);
        }

        private void Unget(IConnection connection)
        {
            lock (pool)
                pool.AddLast(connection);
        }

        private class PooledConnectionWrapper : IConnection
        {
            private ConnectionPoolingConnectionFactory connectionPool;
            private IConnection innerConnection;
            private bool disposed = false;

            public PooledConnectionWrapper(ConnectionPoolingConnectionFactory connectionPool, IConnection innerConnection)
            {
                this.connectionPool = connectionPool;
                this.innerConnection = innerConnection;
            }

            #region IDisposable implementation

            public void Dispose()
            {
                if (!disposed)
                {
                    this.disposed = true;
                    this.connectionPool.Unget(innerConnection);
                }
            }

            #endregion
            #region IConnection implementation

            public Task<T> RunAsync<T>(IQueryConverter queryConverter, IScalarQuery<T> queryObject, CancellationToken cancellationToken)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<T>(queryConverter, queryObject, cancellationToken);
            }

            public IAsyncEnumerator<T> RunAsync<T>(IQueryConverter queryConverter, ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<T>(queryConverter, queryObject);
            }

            // Hm... doesn't really seem like you'd want to set these properties on a pooled connection.  Not sure
            // what the correct solution is to that, but to prevent possibly confusing behavior, we'll just throw
            // an error on set.

            public IQueryConverter QueryConverter
            {
                get { return this.innerConnection.QueryConverter; }
                set { throw new NotSupportedException("set_QueryConverter not supported on pooled connection"); }
            }

            public ILogger Logger
            {
                get { return this.innerConnection.Logger; }
                set { throw new NotSupportedException("set_Logger not supported on pooled connection"); }
            }

            public TimeSpan QueryTimeout
            {
                get { return this.innerConnection.QueryTimeout; }
                set { throw new NotSupportedException("set_QueryTimeout not supported on pooled connection"); }
            }

            #endregion
        }
    }
}

