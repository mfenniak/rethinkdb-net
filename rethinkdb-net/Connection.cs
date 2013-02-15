using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace RethinkDb
{
    sealed class Connection : IDisposable
    {
        private static TaskFactory taskFactory = new TaskFactory();
        private static TimeSpan connectTimeout = TimeSpan.FromSeconds(30);
        private static TimeSpan runQueryTimeout = TimeSpan.FromSeconds(30);
        private static byte[] connectHeader = new byte[] { 0x35, 0xba, 0x61, 0xaf, };

        private Socket socket;
        private NetworkStream stream;

        #region IDisposable Members

        public void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            if (socket != null)
            {
                socket.Close();
                socket.Dispose();
                socket = null;
            }
        }

        #endregion

        public async Task Connect(params string[] hostname)
        {
            var cancellationToken = new CancellationTokenSource(connectTimeout).Token;

            foreach (var h in hostname)
            {
                IPAddress[] addresses;
                try
                {
                    addresses = await Dns.GetHostAddressesAsync(h);
                }
                catch (Exception)
                {
                    continue;
                }

                foreach (var a in addresses)
                {
                    try
                    {
                        await DoTryConnect(a, cancellationToken);
                        return;
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            // FIXME: Custom exception class
            throw new Exception("Failed to resolve a connectable address.");
        }

        private async Task DoTryConnect(IPAddress address, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            Socket socket = null;
            NetworkStream stream = null;

            try
            {
                socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await taskFactory.FromAsync(
                    (asyncCallback, asyncState) => socket.BeginConnect(address, 28015, asyncCallback, asyncState),
                    ar => socket.EndConnect(ar),
                    null
                );

                stream = new NetworkStream(socket, true);
                await stream.WriteAsync(connectHeader, 0, connectHeader.Length, cancellationToken);

                this.socket = socket;
                this.stream = stream;
            }
            catch (Exception)
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
                if (socket != null)
                {
                    socket.Close();
                    socket.Dispose();
                }
                throw;
            }
        }

        private async Task ReadMyBytes(byte[] buffer, CancellationToken cancellationToken)
        {
            int totalBytesRead = 0;
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, buffer.Length, cancellationToken);
                totalBytesRead += bytesRead;

                if (bytesRead == 0)
                    throw new EndOfStreamException("Network stream closed while attempting to read.");
                else if (totalBytesRead == buffer.Length)
                    break;
            }
        }

        private async Task<Response> InternalRunQuery()
        {
            var cancellationToken = new CancellationTokenSource(runQueryTimeout).Token;

            var query = new Query();
            query.token = 1;
            query.type = Query.QueryType.READ;
            query.read_query = new ReadQuery();
            query.read_query.term = new Term();
            query.read_query.term.type = Term.TermType.TABLE;
            query.read_query.term.table = new Term.Table()
            {
                table_ref = new TableRef()
                {
                    db_name = "voicemail",
                    table_name = "user",
                }
            };

            using (var memoryBuffer = new MemoryStream(1024))
            {
                Serializer.Serialize(memoryBuffer, query);

                var data = memoryBuffer.ToArray();
                var header = BitConverter.GetBytes(data.Length);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(header, 0, header.Length);

                await stream.WriteAsync(header, 0, header.Length, cancellationToken);
                await stream.WriteAsync(data, 0, data.Length, cancellationToken);
            }

            byte[] headerSize = new byte[4];
            await ReadMyBytes(headerSize, cancellationToken);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(headerSize, 0, headerSize.Length);
            var respSize = BitConverter.ToInt32(headerSize, 0);

            byte[] retVal = new byte[respSize];
            await ReadMyBytes(retVal, cancellationToken);
            using (var memoryBuffer = new MemoryStream(retVal))
            {
                var response = Serializer.Deserialize<Response>(memoryBuffer);
                return response;
            }
        }

        public async Task<T> FetchSingleObject<T>(IJsonSerializer<T> serializer)
        {
            var response = await InternalRunQuery();

            switch (response.status_code)
            {
                case Response.StatusCode.SUCCESS_EMPTY:
                    throw new InvalidOperationException("Expected 1 object, received 0");
                case Response.StatusCode.SUCCESS_JSON:
                case Response.StatusCode.SUCCESS_PARTIAL:
                case Response.StatusCode.SUCCESS_STREAM:
                    if (response.response.Count != 1)
                        throw new InvalidOperationException(String.Format("Expected 1 object, received {0}", response.response.Count));
                    return serializer.Deserialize(response.response[0]);
                default:
                    // FIXME: error handling
                    throw new Exception("error status code");
            }
        }
    }
}
