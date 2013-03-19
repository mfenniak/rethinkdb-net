using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;

namespace RethinkDb
{
    sealed class Connection : IDisposable
    {
        private static TaskFactory taskFactory = new TaskFactory();
        private static TimeSpan connectTimeout = TimeSpan.FromSeconds(30);
        private static TimeSpan runQueryTimeout = TimeSpan.FromSeconds(30);
        private static byte[] connectHeader = null;

        private Socket socket;
        private NetworkStream stream;

        public Connection()
        {
            DatumConverterFactory = new DataContractDatumConverterFactory();
        }

        public IDatumConverterFactory DatumConverterFactory
        {
            get;
            set;
        }

        public async Task Connect(params EndPoint[] endpoints)
        {
            var cancellationToken = new CancellationTokenSource(connectTimeout).Token;

            foreach (var ep in endpoints)
            {
                IEnumerable<IPEndPoint> resolvedIpEndpoints = null;
                if (ep is DnsEndPoint)
                {
                    var dnsEndpoint = (DnsEndPoint)ep;
                    try
                    {
                        var ips = await Dns.GetHostAddressesAsync(dnsEndpoint.Host);
                        resolvedIpEndpoints = ips.Select(ip => new IPEndPoint(ip, dnsEndpoint.Port));
                    }
                    catch (Exception)
                    {
                        // FIXME: Log: DNS resolution failed
                        continue;
                    }
                }
                else if (ep is IPEndPoint)
                {
                    resolvedIpEndpoints = Enumerable.Repeat((IPEndPoint)ep, 1);
                }
                else
                {
                    // FIXME: custom exception
                    throw new ArgumentException("Unexpected type of System.Net.EndPoint");
                }

                foreach (var ipEndpoint in resolvedIpEndpoints)
                {
                    try
                    {
                        await DoTryConnect(ipEndpoint, cancellationToken);
                        return;
                    }
                    catch (TaskCanceledException)
                    {
                        // FIXME: Log: timeout occurred trying to connect
                        throw;
                    }
                    catch (Exception)
                    {
                        // FIXME: Log: exception occurred trying to connect
                        continue;
                    }
                }
            }

            // FIXME: Custom exception class
            throw new Exception("Failed to resolve a connectable address.");
        }

        private async Task DoTryConnect(IPEndPoint endpoint, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            Socket socket = null;
            NetworkStream stream = null;

            try
            {
                socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                await taskFactory.FromAsync(
                    (asyncCallback, asyncState) => socket.BeginConnect(endpoint.Address, endpoint.Port, asyncCallback, asyncState),
                    ar => socket.EndConnect(ar),
                    null
                );

                if (connectHeader == null)
                {
                    var header = BitConverter.GetBytes((int)Version.V0_1);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(header, 0, header.Length);
                    connectHeader = header;
                }

                stream = new NetworkStream(socket, true);
                await stream.WriteAsync(connectHeader, 0, connectHeader.Length, cancellationToken);

                this.socket = socket;
                this.stream = stream;
            }
            catch (Exception)
            {
                if (stream != null)
                {
                    try
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }
                if (socket != null)
                {
                    try
                    {
                        socket.Close();
                        socket.Dispose();
                    }
                    catch (Exception)
                    {
                    }
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

            var dbTerm = new Term() {
                type = Term.TermType.DB,
            };
            dbTerm.args.Add(
                new Term() {
                    type = Term.TermType.DATUM,
                    datum = new Datum() {
                        type = Datum.DatumType.R_STR,
                        r_str = "voicemail",
                    }
                }
            );

            var tableTerm = new Term() {
                type = Term.TermType.TABLE,
            };
            tableTerm.args.Add(dbTerm);
            tableTerm.args.Add(
                new Term() {
                    type = Term.TermType.DATUM,
                    datum = new Datum() {
                        type = Datum.DatumType.R_STR,
                        r_str = "user",
                    }
                }
            );
            tableTerm.optargs.Add(new Term.AssocPair() {
                key = "use_outdated",
                val = new Term() {
                    type = Term.TermType.DATUM,
                    datum = new Datum() {
                        type = Datum.DatumType.R_BOOL,
                        r_bool = false,
                    }
                }
            });

            var query = new Query();
            query.token = 1;
            query.type = Query.QueryType.START;
            query.query = tableTerm;

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

        public async Task<T> FetchSingleObject<T>(IDatumConverter<T> converter)
        {
            var response = await InternalRunQuery();

            switch (response.type)
            {
                case Response.ResponseType.SUCCESS_SEQUENCE:
                    if (response.response.Count != 1)
                        throw new InvalidOperationException(String.Format("Expected 1 object, received {0}", response.response.Count));
                    return converter.ConvertDatum(response.response[0]);
                case Response.ResponseType.CLIENT_ERROR:
                case Response.ResponseType.COMPILE_ERROR:
                case Response.ResponseType.RUNTIME_ERROR:
                    // FIXME: more robust error handling
                    throw new Exception("Error: " + response.response[0].r_str);
                default:
                    throw new InvalidOperationException("Unhandled response type in FetchSingleObject<T>");
            }
        }

        public Task<T> FetchSingleObject<T>()
        {
            return FetchSingleObject<T>(DatumConverterFactory.Get<T>());
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (stream != null)
            {
                try
                {
                    stream.Close();
                    stream.Dispose();
                }
                catch (Exception)
                {
                }
                stream = null;
            }
            if (socket != null)
            {
                try
                {
                    socket.Close();
                    socket.Dispose();
                }
                catch (Exception)
                {
                }
                socket = null;
            }
        }

        #endregion
    }
}
