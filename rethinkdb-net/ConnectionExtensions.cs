using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RethinkDb
{
    public static class ConnectionExtensions
    {
        #region IConnection minimalism

        public static Task<T> RunAsync<T>(this IConnection connection, IScalarQuery<T> queryObject, IQueryConverter queryConverter = null, CancellationToken? cancellationToken = null)
        {
            if (queryConverter == null)
                queryConverter = connection.QueryConverter;
            if (!cancellationToken.HasValue)
                cancellationToken = new CancellationTokenSource(connection.QueryTimeout).Token;
            return connection.RunAsync<T>(queryConverter, queryObject, cancellationToken.Value);
        }

        public static IAsyncEnumerator<T> RunAsync<T>(this IConnection connection, ISequenceQuery<T> queryObject, IQueryConverter queryConverter = null)
        {
            if (queryConverter == null)
                queryConverter = connection.QueryConverter;
            return connection.RunAsync<T>(queryConverter, queryObject);
        }

        public static IAsyncEnumerator<T> StreamChangesAsync<T>(this IConnection connection, IStreamingSequenceQuery<T> queryObject, IQueryConverter queryConverter = null)
        {
            if (queryConverter == null)
                queryConverter = connection.QueryConverter;
            return new StreamingAsyncEnumeratorWrapper<T>(connection.RunAsync<T>(queryConverter, queryObject));
        }

        #endregion
        #region IConnectableConnection

        public static Task ConnectAsync(this IConnectableConnection connection, CancellationToken? cancellationToken = null)
        {
            if (!cancellationToken.HasValue)
                cancellationToken = new CancellationTokenSource(connection.ConnectTimeout).Token;
            return connection.ConnectAsync(cancellationToken.Value);
        }

        #endregion
        #region IAsyncEnumerator minimalism

        private static CancellationToken MakeDefaultCancellationToken<T>(IAsyncEnumerator<T> asyncEnumerator)
        {
            if (asyncEnumerator is StreamingAsyncEnumeratorWrapper<T>)
                return new CancellationTokenSource().Token;
            else
                return new CancellationTokenSource(asyncEnumerator.Connection.QueryTimeout).Token;
        }

        public static Task<bool> MoveNext<T>(this IAsyncEnumerator<T> asyncEnumerator, CancellationToken? cancellationToken = null)
        {
            if (!cancellationToken.HasValue)
                cancellationToken = MakeDefaultCancellationToken(asyncEnumerator);
            return asyncEnumerator.MoveNext(cancellationToken.Value);
        }

        public static Task Dispose<T>(this IAsyncEnumerator<T> asyncEnumerator, CancellationToken? cancellationToken = null)
        {
            if (!cancellationToken.HasValue)
                cancellationToken = MakeDefaultCancellationToken(asyncEnumerator);
            return asyncEnumerator.Dispose(cancellationToken.Value);
        }

        #endregion

        sealed class StreamingAsyncEnumeratorWrapper<T> : IAsyncEnumerator<T>
        {
            private IAsyncEnumerator<T> innerEnumerator;

            public StreamingAsyncEnumeratorWrapper(IAsyncEnumerator<T> innerEnumerator)
            {
                if (innerEnumerator == null)
                    throw new ArgumentNullException("innerEnumerator");
                this.innerEnumerator = innerEnumerator;
            }

            #region IAsyncEnumerator implementation

            public void Reset()
            {
                this.innerEnumerator.Reset();
            }

            public Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                return this.innerEnumerator.MoveNext(cancellationToken);
            }

            public Task Dispose(CancellationToken cancellationToken)
            {
                return this.innerEnumerator.Dispose(cancellationToken);
            }

            public IConnection Connection
            {
                get
                {
                    return this.innerEnumerator.Connection;
                }
            }

            public T Current
            {
                get
                {
                    return this.innerEnumerator.Current;
                }
            }

            #endregion
        }
    }
}
