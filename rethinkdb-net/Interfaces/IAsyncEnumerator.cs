using System.Threading;
using System.Threading.Tasks;

namespace RethinkDb
{
    public interface IAsyncEnumerator<T>
    {
        IConnection Connection { get; }
        T Current { get; }
        Task<bool> MoveNext(CancellationToken cancellationToken);
        Task Dispose(CancellationToken cancellationToken);
    }
}
