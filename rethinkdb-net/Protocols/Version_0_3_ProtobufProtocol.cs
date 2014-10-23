using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using ProtoBuf;
using System.Text;
using RethinkDb.Logging;

namespace RethinkDb.Protocols
{
    [ImmutableObject(true)]
    public class Version_0_3_Protobuf : Version_0_3
    {
        public static readonly Version_0_3_Protobuf Instance = new Version_0_3_Protobuf();

        private byte[] protocolHeader;

        private Version_0_3_Protobuf()
        {
            var header = BitConverter.GetBytes((int)Spec.VersionDummy.Protocol.PROTOBUF);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, 0, header.Length);
            protocolHeader = header;
        }

        protected override byte[] ProtocolHeader
        {
            get { return protocolHeader; }
        }

        public override async Task WriteQueryToStream(Stream stream, ILogger logger, Spec.Query query, CancellationToken cancellationToken)
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

        public override async Task<Spec.Response> ReadResponseFromStream(Stream stream, ILogger logger)
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
