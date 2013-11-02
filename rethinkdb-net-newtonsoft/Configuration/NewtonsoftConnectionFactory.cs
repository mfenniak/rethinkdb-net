using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RethinkDb.Newtonsoft.Configuration
{
    public class NewtonsoftConnectionFactory : IConnectionFactory
    {
        public NewtonsoftConnectionFactory( IEnumerable<EndPoint> endPoints )
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

        public async Task<IConnection> GetAsync()
        {
            var connection = new Connection();

            connection.DatumConverterFactory = new NewtonSerializer();

            connection.EndPoints = EndPoints;
            if( Logger != null )
                connection.Logger = Logger;
            if( ConnectTimeout.HasValue )
                connection.ConnectTimeout = ConnectTimeout.Value;
            if( !string.IsNullOrEmpty( AuthorizationKey ) )
                connection.AuthorizationKey = AuthorizationKey;
            await connection.ConnectAsync();
            return connection;
        }
    }
}