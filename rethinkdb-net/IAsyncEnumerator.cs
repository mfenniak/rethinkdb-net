using System.Threading.Tasks;

namespace RethinkDb
{
    public interface IAsyncEnumerator<T>
    {
        T Current { get; }
        Task<bool> MoveNext();
    }
}

