using System.Threading.Tasks;
using System.Collections.Generic;
using System;

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
            }

            public void Reconnect()
            {
                // FIXME: This seems like it's not thread-safe; I know we just got a network error, but could
                // this connection still be in-use?  Also, Reconnect() could be called simultaneously from
                // multiple users... probably won't be a serious problem when used in combination with the connection
                // pool, as connections are less likely to be shared across threads.
                this.innerConnection.Dispose();
                this.innerConnection = this.reliableConnectionFactory.innerConnectionFactory.Get();
            }

            private async Task<TReturnValue> RetryRunAsync<TReturnValue>(Func<Task<TReturnValue>> action)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ReliableConnectionWrapper");
                try
                {
                    return await action();
                }
                catch (RethinkDbNetworkException)
                {
                    Reconnect();
                }
                return await action();
            }

            private TReturnValue RetryRun<TReturnValue>(Func<TReturnValue> action)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ReliableConnectionWrapper");
                try
                {
                    return action();
                }
                catch (RethinkDbNetworkException)
                {
                    Reconnect();
                }
                return action();
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

            public Task<T> RunAsync<T>(ISingleObjectQuery<T> queryObject)
            {
                return RetryRunAsync(() => this.innerConnection.RunAsync<T>(queryObject));
            }

            public Task<TResponseType> RunAsync<TResponseType>(IDatumConverterFactory datumConverterFactory, IWriteQuery<TResponseType> queryObject)
            {
                return RetryRunAsync(() => this.innerConnection.RunAsync<TResponseType>(datumConverterFactory, queryObject));
            }

            public Task<TResponseType> RunAsync<TResponseType>(IWriteQuery<TResponseType> queryObject)
            {
                return RetryRunAsync(() => this.innerConnection.RunAsync<TResponseType>(queryObject));
            }

            public IAsyncEnumerator<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ReliableConnectionWrapper");
                return new RetryAsyncEnumeratorWrapper<T>(this, () => this.innerConnection.RunAsync(datumConverterFactory, queryObject));
            }

            public IAsyncEnumerator<T> RunAsync<T>(ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ReliableConnectionWrapper");
                return new RetryAsyncEnumeratorWrapper<T>(this, () => this.innerConnection.RunAsync(queryObject));
            }

            public T Run<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject)
            {
                return RetryRun(() => this.innerConnection.Run<T>(datumConverterFactory, queryObject));
            }

            public T Run<T>(ISingleObjectQuery<T> queryObject)
            {
                return RetryRun(() => this.innerConnection.Run<T>(queryObject));
            }

            public TResponseType Run<TResponseType>(IDatumConverterFactory datumConverterFactory, IWriteQuery<TResponseType> queryObject)
            {
                return RetryRun(() => this.innerConnection.Run<TResponseType>(datumConverterFactory, queryObject));
            }

            public TResponseType Run<TResponseType>(IWriteQuery<TResponseType> queryObject)
            {
                return RetryRun(() => this.innerConnection.Run<TResponseType>(queryObject));
            }

            public IEnumerable<T> Run<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ReliableConnectionWrapper");
                // Hm... we so wouldn't have to implement this if IConnection only had an asynchronous API, with a
                // synchronous API provided by extension methods... will delay until that is done.
                throw new System.NotImplementedException();
            }

            public IEnumerable<T> Run<T>(ISequenceQuery<T> queryObject)
            {
                if (this.disposed)
                    throw new ObjectDisposedException("ReliableConnectionWrapper");
                // Hm... we so wouldn't have to implement this if IConnection only had an asynchronous API, with a
                // synchronous API provided by extension methods... will delay until that is done.
                throw new System.NotImplementedException();
            }

            public IDatumConverterFactory DatumConverterFactory
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                    throw new System.NotImplementedException();
                }
            }

            public ILogger Logger
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                    throw new System.NotImplementedException();
                }
            }

            public TimeSpan QueryTimeout
            {
                get
                {
                    throw new System.NotImplementedException();
                }
                set
                {
                    throw new System.NotImplementedException();
                }
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
                    catch (RethinkDbNetworkException)
                    {
                        this.innerEnumerator = null;
                        this.reliableConnection.Reconnect();
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

