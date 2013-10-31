using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RethinkDb.ConnectionFactories
{
    /// <summary>
    /// Most basic implementation of an IConnectionFactory, allowing configuration of all connectable properties of
    /// the connections being created.
    /// </summary>
    public class DefaultConnectionFactory : IConnectionFactory
    {
        public DefaultConnectionFactory(IEnumerable<EndPoint> endPoints)
        {
            this.EndPoints = endPoints;
        }

        public IEnumerable<EndPoint> EndPoints
        {
            get;
            set;
        }

        public TimeSpan? ConnectTimeout
        {
            get;
            set;
        }

        public string AuthorizationKey
        {
            get;
            set;
        }

        public ILogger Logger
        {
            get;
            set;
        }

        public virtual async Task<IConnection> GetAsync()
        {
            var connection = new Connection();
            connection.EndPoints = EndPoints;
            if (Logger != null)
                connection.Logger = Logger;
            if (ConnectTimeout.HasValue)
                connection.ConnectTimeout = ConnectTimeout.Value;
            if (!string.IsNullOrEmpty(AuthorizationKey))
                connection.AuthorizationKey = AuthorizationKey;
            await connection.ConnectAsync();
            return connection;
        }
    }
}

