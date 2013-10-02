using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RethinkDb
{
    public interface IConnectableConnection : IConnection
    {
        IEnumerable<EndPoint> EndPoints
        {
            get;
            set;
        }

        TimeSpan ConnectTimeout
        {
            get;
            set;
        }

        string AuthorizationKey
        {
            get;
            set;
        }

        Task ConnectAsync();
    }
}

