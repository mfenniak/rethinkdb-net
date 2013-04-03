using System;
using System.Net;
using System.Threading.Tasks;

namespace RethinkDb
{
    public interface IAsyncConnection : IDisposable
    {
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

        Task ConnectAsync(params EndPoint[] endpoints);

        Task<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject);

        Task<T> RunAsync<T>(ISingleObjectQuery<T> queryObject);

        IAsyncEnumerator<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject);

        IAsyncEnumerator<T> RunAsync<T>(ISequenceQuery<T> queryObject);

        Task<DmlResponse> RunAsync(IDatumConverterFactory datumConverterFactory, IDmlQuery queryObject);

        Task<DmlResponse> RunAsync(IDmlQuery queryObject);

        Task<DmlResponse> RunAsync<T>(IDatumConverterFactory datumConverterFactory, IWriteQuery<T> queryObject);

        Task<DmlResponse> RunAsync<T>(IWriteQuery<T> queryObject);
    }
}

