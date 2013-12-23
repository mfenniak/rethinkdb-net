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

        public ConnectionPoolingConnectionFactory(IConnectionFactory innerConnectionFactory)
        {
            this.innerConnectionFactory = innerConnectionFactory;
        }

        public async Task<IConnection> GetAsync()
        {
            lock (pool)
            {
                var node = pool.First;
                if (node != null)
                {
                    pool.Remove(node);
                    return new PooledConnectionWrapper(this, node.Value);
                }
            }

            // Couldn't get a connection from the pool, so create a new one.
            return new PooledConnectionWrapper(this, await innerConnectionFactory.GetAsync());
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

            public Task<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory, IScalarQuery<T> queryObject, CancellationToken cancellationToken)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<T>(datumConverterFactory, expressionConverterFactory, queryObject, cancellationToken);
            }

            public IAsyncEnumerator<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory, ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<T>(datumConverterFactory, expressionConverterFactory, queryObject);
            }

            // Hm... doesn't really seem like you'd want to set these properties on a pooled connection.  Not sure
            // what the correct solution is to that, but to prevent possibly confusing behavior, we'll just throw
            // an error on set.

            public IDatumConverterFactory DatumConverterFactory
            {
                get { return this.innerConnection.DatumConverterFactory; }
                set { throw new NotSupportedException("set_DatumConverterFactory not supported on pooled connection"); }
            }

            public IExpressionConverterFactory ExpressionConverterFactory
            {
                get { return this.innerConnection.ExpressionConverterFactory; }
                set { throw new NotSupportedException("set_LambdaTermConverterFactory not supported on pooled connection"); }
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

