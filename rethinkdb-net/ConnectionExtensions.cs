using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RethinkDb
{
    public static class ConnectionExtensions
    {
        #region IConnection minimalism

        public static Task<T> RunAsync<T>(this IConnection connection, IScalarQuery<T> queryObject)
        {
            return connection.RunAsync<T>(connection.DatumConverterFactory, queryObject);
        }

        public static IAsyncEnumerator<T> RunAsync<T>(this IConnection connection, ISequenceQuery<T> queryObject)
        {
            return connection.RunAsync<T>(connection.DatumConverterFactory, queryObject);
        }

        #endregion
    }
}

