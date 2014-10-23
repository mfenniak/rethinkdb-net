using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using RethinkDb.Logging;
using System.IO;

namespace RethinkDb.Protocols
{
    public abstract class Version_0_3 : IProtocol
    {
        private byte[] connectHeader;

        protected Version_0_3()
        {
            var header = BitConverter.GetBytes((int)Spec.VersionDummy.Version.V0_3);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, 0, header.Length);
            connectHeader = header;
        }

        protected abstract byte[] ProtocolHeader { get; }
        public abstract Task WriteQueryToStream(Stream stream, ILogger logger, Spec.Query query, CancellationToken cancellationToken);
        public abstract Task<Spec.Response> ReadResponseFromStream(Stream stream, ILogger logger);

        public async Task ConnectionHandshake(Stream stream, ILogger logger, string authorizationKey, CancellationToken cancellationToken)
        {
            await stream.WriteAsync(connectHeader, 0, connectHeader.Length, cancellationToken);
            logger.Debug("Sent ReQL header");

            if (String.IsNullOrEmpty(authorizationKey))
            {
                await stream.WriteAsync(new byte[] { 0, 0, 0, 0 }, 0, 4, cancellationToken);
            }
            else
            {
                var keyInBytes = Encoding.UTF8.GetBytes(authorizationKey);
                var authKeyLength = BitConverter.GetBytes(keyInBytes.Length);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(authKeyLength, 0, authKeyLength.Length);
                await stream.WriteAsync(authKeyLength, 0, authKeyLength.Length);
                await stream.WriteAsync(keyInBytes, 0, keyInBytes.Length);
            }

            await stream.WriteAsync(ProtocolHeader, 0, ProtocolHeader.Length, cancellationToken);

            byte[] authReponseBuffer = new byte[1024];
            var authResponseLength = await stream.ReadUntilNullTerminator(logger, authReponseBuffer, cancellationToken);
            var authResponse = Encoding.ASCII.GetString(authReponseBuffer, 0, authResponseLength);
            if (authResponse != "SUCCESS")
                throw new RethinkDbRuntimeException("Unexpected authentication response; expected SUCCESS but got: " + authResponse);
        }
    }
}
