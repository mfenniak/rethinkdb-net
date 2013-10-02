using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using RethinkDb.Logging;

namespace RethinkDb.ConnectionFactories
{
    /// <summary>
    /// An implementation of IConnectionFactory that returns wrapped connections that provide retry capabilities if
    /// network errors occur.
    /// </summary>
    public class ReliableConnectionFactory : IConnectionFactory
    {
        private IConnectionFactory innerConnectionFactory;

        public ReliableConnectionFactory(IConnectionFactory innerConnectionFactory)
        {
            this.innerConnectionFactory = innerConnectionFactory;
        }

        public async Task<IConnection> GetAsync()
        {
            return new ReliableConnectionWrapper(this, await innerConnectionFactory.GetAsync());
        }

        private class ReliableConnectionWrapper : IConnection
        {
            private ReliableConnectionFactory reliableConnectionFactory;
            private IConnection innerConnection;
            private bool disposed = false;

            public ReliableConnectionWrapper(ReliableConnectionFactory reliableConnectionFactory, IConnection innerConnection)
            {
                this.reliableConnectionFactory = reliableConnectionFactory;
                this.innerConnection = innerConnection;

                this.DatumConverterFactory = innerConnection.DatumConverterFactory;
                this.Logger = innerConnection.Logger;
                this.QueryTimeout = innerConnection.QueryTimeout;
            }

            public void Reconnect(RethinkDbNetworkException e)
            {
                Logger.Warning("Attempting to reconnect to RethinkDB after network exception: {0}", e);

                // FIXME: This seems like it's not thread-safe; I know we just got a network error, but could
                // this connection still be in-use?  Also, Reconnect() could be called simultaneously from
                // multiple users... probably won't be a serious problem when used in combination with the connection
                // pool, as connections are less likely to be shared across threads.
                this.innerConnection.Dispose();
                this.innerConnection = this.reliableConnectionFactory.innerConnectionFactory.Get();

                // Re-set the new connection's properties to match this one's.
                innerConnection.DatumConverterFactory = this.DatumConverterFactory;
                innerConnection.Logger = this.Logger;
                innerConnection.QueryTimeout = this.QueryTimeout;
            }

            private async Task<TReturnValue> RetryRunAsync<TReturnValue>(Func<Task<TReturnValue>> action)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ReliableConnectionWrapper");
                try
                {
                    return await action();
                }
                catch (RethinkDbNetworkException e)
                {
                    Reconnect(e);
                }
                return await action();
            }

            #region IDisposable implementation

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.innerConnection.Dispose();
                    this.innerConnection = null;
                    this.reliableConnectionFactory = null;
                }
            }

            #endregion
            #region IConnection implementation

            public Task<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject)
            {
                return RetryRunAsync(() => this.innerConnection.RunAsync<T>(datumConverterFactory, queryObject));
            }

            public Task<TResponseType> RunAsync<TResponseType>(IDatumConverterFactory datumConverterFactory, IWriteQuery<TResponseType> queryObject)
            {
                return RetryRunAsync(() => this.innerConnection.RunAsync<TResponseType>(datumConverterFactory, queryObject));
            }

            public IAsyncEnumerator<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ReliableConnectionWrapper");
                return new RetryAsyncEnumeratorWrapper<T>(this, () => this.innerConnection.RunAsync(datumConverterFactory, queryObject));
            }

            public IDatumConverterFactory DatumConverterFactory
            {
                get;
                set;
            }

            public ILogger Logger
            {
                get;
                set;
            }

            public TimeSpan QueryTimeout
            {
                get;
                set;
            }

            #endregion
        }

        #region RetryAsyncEnumeratorWrapper

        private class RetryAsyncEnumeratorWrapper<T> : IAsyncEnumerator<T>
        {
            private ReliableConnectionWrapper reliableConnection;
            private Func<IAsyncEnumerator<T>> enumeratorConstructor;
            private IAsyncEnumerator<T> innerEnumerator;

            public RetryAsyncEnumeratorWrapper(ReliableConnectionWrapper reliableConnection, Func<IAsyncEnumerator<T>> enumeratorConstructor)
            {
                this.reliableConnection = reliableConnection;
                this.enumeratorConstructor = enumeratorConstructor;
            }

            #region IAsyncEnumerator implementation

            public async Task<bool> MoveNext()
            {
                if (this.innerEnumerator == null)
                {
                    // First ever MoveNext(); this one we'll retry if it fails.  Others we won't, because we don't
                    // know what page of a query we might be on.
                    try
                    {
                        this.innerEnumerator = this.enumeratorConstructor();
                        return await this.innerEnumerator.MoveNext();
                    }
                    catch (RethinkDbNetworkException e)
                    {
                        this.innerEnumerator = null;
                        this.reliableConnection.Reconnect(e);
                    }

                    this.innerEnumerator = this.enumeratorConstructor();
                    return await this.innerEnumerator.MoveNext();
                }
                else
                {
                    return await this.innerEnumerator.MoveNext();
                }
            }

            public async Task Dispose()
            {
                if (this.innerEnumerator != null)
                    await this.innerEnumerator.Dispose();
            }

            public T Current
            {
                get
                {
                    if (this.innerEnumerator == null)
                        throw new InvalidOperationException("Call MoveNext first");
                    return this.innerEnumerator.Current;
                }
            }

            #endregion
        }

        #endregion
    }
}

