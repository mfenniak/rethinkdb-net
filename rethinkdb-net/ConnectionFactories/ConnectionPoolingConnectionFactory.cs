using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace RethinkDb
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

        public IConnection Get()
        {
            return TaskUtilities.ExecuteSynchronously(() => GetAsync());
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

            public Task<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<T>(datumConverterFactory, queryObject);
            }

            public Task<T> RunAsync<T>(ISingleObjectQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<T>(queryObject);
            }

            public IAsyncEnumerator<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<T>(datumConverterFactory, queryObject);
            }

            public IAsyncEnumerator<T> RunAsync<T>(ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<T>(queryObject);
            }

            public Task<TResponseType> RunAsync<TResponseType>(IDatumConverterFactory datumConverterFactory, IWriteQuery<TResponseType> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<TResponseType>(datumConverterFactory, queryObject);
            }

            public Task<TResponseType> RunAsync<TResponseType>(IWriteQuery<TResponseType> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.RunAsync<TResponseType>(queryObject);
            }

            public T Run<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.Run<T>(datumConverterFactory, queryObject);
            }

            public T Run<T>(ISingleObjectQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.Run<T>(queryObject);
            }

            public IEnumerable<T> Run<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.Run<T>(datumConverterFactory, queryObject);
            }

            public IEnumerable<T> Run<T>(ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.Run<T>(queryObject);
            }

            public TResponseType Run<TResponseType>(IDatumConverterFactory datumConverterFactory, IWriteQuery<TResponseType> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.Run<TResponseType>(datumConverterFactory, queryObject);
            }

            public TResponseType Run<TResponseType>(IWriteQuery<TResponseType> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("PooledConnectionWrapper");
                return this.innerConnection.Run<TResponseType>(queryObject);
            }

            // Hm... doesn't really seem like you'd want to set these properties on a pooled connection.  Not sure
            // what the correct solution is to that, but to prevent possibly confusing behavior, we'll just throw
            // an error on set.

            public IDatumConverterFactory DatumConverterFactory
            {
                get { return this.innerConnection.DatumConverterFactory; }
                set { throw new NotSupportedException("set_DatumConverterFactory not supported on pooled connection"); }
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

