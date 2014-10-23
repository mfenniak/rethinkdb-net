using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RethinkDb.Logging;

namespace RethinkDb.Protocols
{
    public static class Extensions
    {
        public static async Task<int> ReadUntilNullTerminator(this Stream stream, ILogger logger, byte[] buffer, CancellationToken cancellationToken)
        {
            int totalBytesRead = 0;
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead, cancellationToken);
                totalBytesRead += bytesRead;
                logger.Debug("Received {0} / {1} bytes in NullTerminator buffer", bytesRead, buffer.Length);

                if (bytesRead == 0)
                    throw new RethinkDbNetworkException("Network stream closed while attempting to read");
                else if (buffer[totalBytesRead - 1] == 0)
                    return totalBytesRead - 1;
                else if (totalBytesRead == buffer.Length)
                    throw new RethinkDbNetworkException("Ran out of space in buffer while looking for a null-terminated string");
            }
        }

        public static async Task ReadMyBytes(this Stream stream, ILogger logger, byte[] buffer)
        {
            int totalBytesRead = 0;
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead);
                totalBytesRead += bytesRead;
                logger.Debug("Received {0} / {1} bytes of packet", bytesRead, buffer.Length);

                if (bytesRead == 0)
                    throw new RethinkDbNetworkException("Network stream closed while attempting to read");
                else if (totalBytesRead == buffer.Length)
                    break;
            }
        }
    }
}

