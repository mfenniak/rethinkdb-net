using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace RethinkDb.Protocols
{
    public interface IProtocol
    {
        Task ConnectionHandshake(Stream stream, ILogger logger, string authorizationKey, CancellationToken cancellationToken);
        Task WriteQueryToStream(Stream stream, ILogger logger, Spec.Query query, CancellationToken cancellationToken);
        Task<Spec.Response> ReadResponseFromStream(Stream stream, ILogger logger);
    }
}
