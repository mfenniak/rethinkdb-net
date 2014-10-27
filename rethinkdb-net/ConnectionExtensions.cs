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

        public static Task<bool> MoveNext<T>(this IAsyncEnumerator<T> asyncEnumerator, CancellationToken? cancellationToken = null)
        {
            if (!cancellationToken.HasValue)
                cancellationToken = new CancellationTokenSource(asyncEnumerator.Connection.QueryTimeout).Token;
            return asyncEnumerator.MoveNext(cancellationToken.Value);
        }

        public static Task Dispose<T>(this IAsyncEnumerator<T> asyncEnumerator, CancellationToken? cancellationToken = null)
        {
            if (!cancellationToken.HasValue)
                cancellationToken = new CancellationTokenSource(asyncEnumerator.Connection.QueryTimeout).Token;
            return asyncEnumerator.Dispose(cancellationToken.Value);
        }

        #endregion
    }
}
