using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RethinkDb
{
    public static class ConnectionExtensions
    {
        #region IConnection minimalism

        public static Task<T> RunAsync<T>(this IConnection connection, IScalarQuery<T> queryObject, IDatumConverterFactory datumConverterFactory = null, IExpressionConverterFactory expressionConverterFactory = null, CancellationToken? cancellationToken = null)
        {
            if (datumConverterFactory == null)
                datumConverterFactory = connection.DatumConverterFactory;
            if (expressionConverterFactory == null)
                expressionConverterFactory = connection.ExpressionConverterFactory;
            if (!cancellationToken.HasValue)
                cancellationToken = new CancellationTokenSource(connection.QueryTimeout).Token;
            return connection.RunAsync<T>(datumConverterFactory, expressionConverterFactory, queryObject, cancellationToken.Value);
        }

        public static IAsyncEnumerator<T> RunAsync<T>(this IConnection connection, ISequenceQuery<T> queryObject, IDatumConverterFactory datumConverterFactory = null, IExpressionConverterFactory expressionConverterFactory = null)
        {
            if (datumConverterFactory == null)
                datumConverterFactory = connection.DatumConverterFactory;
            if (expressionConverterFactory == null)
                expressionConverterFactory = connection.ExpressionConverterFactory;
            return connection.RunAsync<T>(datumConverterFactory, expressionConverterFactory, queryObject);
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
