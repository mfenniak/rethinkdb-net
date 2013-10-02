using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RethinkDb
{
    public static class ConnectionExtensions
    {
        #region IConnection minimalism

        public static Task<T> RunAsync<T>(this IConnection connection, ISingleObjectQuery<T> queryObject)
        {
            return connection.RunAsync<T>(connection.DatumConverterFactory, queryObject);
        }

        public static IAsyncEnumerator<T> RunAsync<T>(this IConnection connection, ISequenceQuery<T> queryObject)
        {
            return connection.RunAsync<T>(connection.DatumConverterFactory, queryObject);
        }

        public static Task<TResponseType> RunAsync<TResponseType>(this IConnection connection, IWriteQuery<TResponseType> queryObject)
        {
            return connection.RunAsync<TResponseType>(connection.DatumConverterFactory, queryObject);
        }

        #endregion
    }
}

