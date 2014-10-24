using System;
using System.Threading;
using System.Threading.Tasks;

namespace RethinkDb
{
    public interface IConnection : IDisposable
    {
        IQueryConverter QueryConverter
        {
            get;
            set;
        }

        ILogger Logger
        {
            get;
            set;
        }

        TimeSpan QueryTimeout
        {
            get;
            set;
        }

        #region Asynchronous API; synchronous API is provided by extension methods

        Task<T> RunAsync<T>(IQueryConverter queryConverter, IScalarQuery<T> queryObject, CancellationToken cancellationToken);

        IAsyncEnumerator<T> RunAsync<T>(IQueryConverter queryConverter, ISequenceQuery<T> queryObject);

        #endregion
    }
}

