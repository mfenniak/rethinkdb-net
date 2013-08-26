using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RethinkDb
{
    public interface IConnection
    {
        IEnumerable<EndPoint> EndPoints
        {
            get;
            set;
        }

        IDatumConverterFactory DatumConverterFactory
        {
            get;
            set;
        }

        ILogger Logger
        {
            get;
            set;
        }

        TimeSpan ConnectTimeout { get; set; }
        
        TimeSpan QueryTimeout { get; set; }

        #region Asynchronous API

        Task ConnectAsync();

        Task<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject);

        Task<T> RunAsync<T>(ISingleObjectQuery<T> queryObject);

        IAsyncEnumerator<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject);

        IAsyncEnumerator<T> RunAsync<T>(ISequenceQuery<T> queryObject);

        Task<TResponseType> RunAsync<TResponseType>(IDatumConverterFactory datumConverterFactory, IWriteQuery<TResponseType> queryObject);

        Task<TResponseType> RunAsync<TResponseType>(IWriteQuery<TResponseType> queryObject);

        #endregion
        #region Synchronous API

        void Connect();

        T Run<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject);

        T Run<T>(ISingleObjectQuery<T> queryObject);

        IEnumerable<T> Run<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject);

        IEnumerable<T> Run<T>(ISequenceQuery<T> queryObject);

        TResponseType Run<TResponseType>(IDatumConverterFactory datumConverterFactory, IWriteQuery<TResponseType> queryObject);

        TResponseType Run<TResponseType>(IWriteQuery<TResponseType> queryObject);

        #endregion
    }
}

