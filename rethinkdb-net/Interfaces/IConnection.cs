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

        Task<DmlResponse> RunAsync(IDatumConverterFactory datumConverterFactory, IDmlQuery queryObject);

        Task<DmlResponse> RunAsync(IDmlQuery queryObject);

        Task<DmlResponse> RunAsync<T>(IDatumConverterFactory datumConverterFactory, IWriteQuery<T> queryObject);

        Task<DmlResponse> RunAsync<T>(IWriteQuery<T> queryObject);

        #endregion
        #region Synchronous API

        void Connect();

        T Run<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject);

        T Run<T>(ISingleObjectQuery<T> queryObject);

        IEnumerable<T> Run<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject);

        IEnumerable<T> Run<T>(ISequenceQuery<T> queryObject);

        DmlResponse Run(IDatumConverterFactory datumConverterFactory, IDmlQuery queryObject);

        DmlResponse Run(IDmlQuery queryObject);

        DmlResponse Run<T>(IDatumConverterFactory datumConverterFactory, IWriteQuery<T> queryObject);

        DmlResponse Run<T>(IWriteQuery<T> queryObject);

        #endregion
    }
}

