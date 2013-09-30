using System.Threading.Tasks;

namespace RethinkDb
{
    public interface IConnectionFactory
    {
        #region Asynchronous API

        Task<IConnection> GetAsync();

        #endregion
        #region Synchronous API

        IConnection Get();

        #endregion
    }
}
