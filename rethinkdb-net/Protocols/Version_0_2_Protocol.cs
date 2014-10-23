using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using ProtoBuf;
using System.Text;
using RethinkDb.Logging;

namespace RethinkDb.Protocols
{
    [ImmutableObject(true)]
    public class Version_0_2 : IProtocol
    {
        public static readonly Version_0_2 Instance = new Version_0_2();

        private byte[] connectHeader;

        private Version_0_2()
        {
            var header = BitConverter.GetBytes((int)Spec.VersionDummy.Version.V0_2);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, 0, header.Length);
            connectHeader = header;
        }

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

            byte[] authReponseBuffer = new byte[1024];
            var authResponseLength = await stream.ReadUntilNullTerminator(logger, authReponseBuffer, cancellationToken);
            var authResponse = Encoding.ASCII.GetString(authReponseBuffer, 0, authResponseLength);
            if (authResponse != "SUCCESS")
                throw new RethinkDbRuntimeException("Unexpected authentication response; expected SUCCESS but got: " + authResponse);
        }

        public async Task WriteQueryToStream(Stream stream, ILogger logger, Spec.Query query, CancellationToken cancellationToken)
        {
            using (var memoryBuffer = new MemoryStream(1024))
            {
                Serializer.Serialize(memoryBuffer, query);

                var data = memoryBuffer.ToArray();
                var lengthHeader = BitConverter.GetBytes(data.Length);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(lengthHeader, 0, lengthHeader.Length);

                logger.Debug("Writing packet, {0} bytes", data.Length);
                await stream.WriteAsync(lengthHeader, 0, lengthHeader.Length, cancellationToken);
                await stream.WriteAsync(data, 0, data.Length, cancellationToken);
            }
        }

        public async Task<Spec.Response> ReadResponseFromStream(Stream stream, ILogger logger)
        {
            byte[] headerSize = new byte[4];
            await stream.ReadMyBytes(logger, headerSize);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(headerSize, 0, headerSize.Length);
            var respSize = BitConverter.ToInt32(headerSize, 0);
            logger.Debug("Received packet header, packet is {0} bytes", respSize);

            byte[] retVal = new byte[respSize];
            await stream.ReadMyBytes(logger, retVal);
            logger.Debug("Received packet completely");
            using (var memoryBuffer = new MemoryStream(retVal))
            {
                var response = Serializer.Deserialize<Spec.Response>(memoryBuffer);
                logger.Debug("Received packet deserialized to response for query token {0}, type: {1}", response.token, response.type);
                return response;
            }
        }
    }
}
