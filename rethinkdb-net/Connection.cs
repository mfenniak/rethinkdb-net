using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using RethinkDb.Spec;

namespace RethinkDb
{
    public sealed class Connection : IDisposable
    {
        private static TaskFactory taskFactory = new TaskFactory();
        private static TimeSpan connectTimeout = TimeSpan.FromSeconds(30);
        private static TimeSpan runQueryTimeout = TimeSpan.FromSeconds(30);
        private static byte[] connectHeader = null;

        private Socket socket;
        private NetworkStream stream;
        private long nextToken = 1;
        private long writeTokenLock = 0;
        private IDictionary<long, TaskCompletionSource<Response>> tokenResponse = new ConcurrentDictionary<long, TaskCompletionSource<Response>>();

        public Connection()
        {
            DatumConverterFactory = DataContractDatumConverterFactory.Instance;
        }

        public IDatumConverterFactory DatumConverterFactory
        {
            get;
            set;
        }

        public ILogger Logger
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
                        if (Logger.DebugEnabled())
                            Logger.Debug("DNS lookup {0} into: [{1}]", resolvedIpEndpoints.EnumerableToString());
                    }
                    catch (Exception e)
                    {
                        Logger.Warning("Failed to resolve DNS for DnsEndPoint {0}: {1}", dnsEndpoint.Host, e);
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
                    Logger.Error("Unsupported System.Net.EndPoint type ({0}); connection aborted", ep.GetType());
                    throw new NotSupportedException("Unexpected type of System.Net.EndPoint");
                }

                foreach (var ipEndpoint in resolvedIpEndpoints)
                {
                    try
                    {
                        Logger.Debug("Connecting to {0}", ipEndpoint);
                        await DoTryConnect(ipEndpoint, cancellationToken);
                        return;
                    }
                    catch (TaskCanceledException e)
                    {
                        Logger.Error("Timeout occurred while connecting to endpoint {0}: {1}; connection aborted", ipEndpoint, e);
                        throw;
                    }
                    catch (Exception e)
                    {
                        Logger.Warning("Unexpected exception occurred while connecting to endpoint {0}: {1}; connection attempts will continue with any other addresses available", ipEndpoint, e);
                        continue;
                    }
                }
            }

            // FIXME: Custom exception class
            Logger.Error("Failed to resolve or connect to any provided address; connection failed");
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

                Logger.Debug("Connected to {0}", endpoint);

                if (connectHeader == null)
                {
                    var header = BitConverter.GetBytes((int)Spec.VersionDummy.Version.V0_1);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(header, 0, header.Length);
                    connectHeader = header;
                }

                stream = new NetworkStream(socket, true);
                await stream.WriteAsync(connectHeader, 0, connectHeader.Length, cancellationToken);

                Logger.Debug("Sent ReQL header");

                this.socket = socket;
                this.stream = stream;

#pragma warning disable 4014
                ReadLoop();
#pragma warning restore 4014
            }
            catch (Exception e)
            {
                Logger.Error("Unexpected exception occurred while connecting to endpoint {0}: {1}; tearing down connection", endpoint, e);
                if (stream != null)
                {
                    try
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Exception occurred while disposing network stream during exception handling: {0}; probably not important", ex);
                    }
                }
                if (socket != null)
                {
                    try
                    {
                        socket.Close();
                        socket.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Exception occurred while disposing network socket during exception handling: {0}; probably not important", ex);
                    }
                }
                throw;
            }
        }

        private async Task ReadLoop()
        {
            // FIXME: need a graceful way to abort this loop when the Connection is disposed... right now it will
            // probably become unhandled and the task library will shut down the process.

            try
            {
                while (true)
                {
                    byte[] headerSize = new byte[4];
                    await ReadMyBytes(headerSize);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(headerSize, 0, headerSize.Length);
                    var respSize = BitConverter.ToInt32(headerSize, 0);
                    Logger.Debug("Received packet header, packet is {0} bytes", respSize);

                    byte[] retVal = new byte[respSize];
                    await ReadMyBytes(retVal);
                    Logger.Debug("Received packet");
                    using (var memoryBuffer = new MemoryStream(retVal))
                    {
                        var response = Serializer.Deserialize<Response>(memoryBuffer);
                        Logger.Debug("Received response to query token {0}, type: {1}", response.token, response.type);

                        TaskCompletionSource<Response> tcs;
                        if (tokenResponse.TryGetValue(response.token, out tcs))
                        {
                            tokenResponse.Remove(response.token);
                            tcs.SetResult(response);
                        } else
                        {
                            Logger.Warning("Received response to query token {0}, but no handler was waiting for that response.  This can occur if the query times out around the same time a response is received.", response.token);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Fatal("Exception occurred in Connection.ReadLoop: {0}; this connection will no longer function correctly, no error recovery will take place", e);
            }
        }

        private async Task ReadMyBytes(byte[] buffer)
        {
            int totalBytesRead = 0;
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, buffer.Length);
                totalBytesRead += bytesRead;

                if (bytesRead == 0)
                    throw new EndOfStreamException("Network stream closed while attempting to read.");
                else if (totalBytesRead == buffer.Length)
                    break;
            }
        }

        internal long GetNextToken()
        {
            return Interlocked.Increment(ref nextToken);
        }

        internal async Task<Response> InternalRunQuery(Spec.Query query)
        {
            var tcs = new TaskCompletionSource<Response>();
            tokenResponse[query.token] = tcs;

            var cancellationToken = new CancellationTokenSource(runQueryTimeout).Token;
            Action abortToken = () => {
                Logger.Warning("Query token {0} timed out after {1}", query.token, runQueryTimeout);
                if (tokenResponse.Remove(query.token))
                    tcs.SetCanceled();
            };
            using (cancellationToken.Register(abortToken))
            {
                using (var memoryBuffer = new MemoryStream(1024))
                {
                    Serializer.Serialize(memoryBuffer, query);

                    var data = memoryBuffer.ToArray();
                    var header = BitConverter.GetBytes(data.Length);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(header, 0, header.Length);

                    // Put query.token into writeTokenLock if writeTokenLock is 0 (ie. unlocked).  If it's not 0,
                    // spin-lock on the compare exchange.
                    while (Interlocked.CompareExchange(ref writeTokenLock, (long)query.token, 0) != 0)
                        ;

                    try
                    {
                        Logger.Debug("Writing packet, {0} bytes", data.Length);
                        await stream.WriteAsync(header, 0, header.Length, cancellationToken);
                        await stream.WriteAsync(data, 0, data.Length, cancellationToken);
                    }
                    finally
                    {
                        // Revert writeTokenLock to 0.
                        writeTokenLock = 0;
                    }
                }

                return await tcs.Task;
            }
        }

        public async Task<T> Run<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject)
        {
            var query = new Spec.Query();
            query.token = GetNextToken();
            query.type = Spec.Query.QueryType.START;
            query.query = queryObject.GenerateTerm(datumConverterFactory);

            var response = await InternalRunQuery(query);

            switch (response.type)
            {
                case Response.ResponseType.SUCCESS_SEQUENCE:
                case Response.ResponseType.SUCCESS_ATOM:
                    if (response.response.Count != 1)
                        throw new InvalidOperationException(String.Format("Expected 1 object, received {0}", response.response.Count));
                    return datumConverterFactory.Get<T>().ConvertDatum(response.response[0]);
                case Response.ResponseType.CLIENT_ERROR:
                case Response.ResponseType.COMPILE_ERROR:
                case Response.ResponseType.RUNTIME_ERROR:
                    // FIXME: more robust error handling
                    throw new Exception("Error: " + response.response[0].r_str);
                default:
                    throw new InvalidOperationException("Unhandled response type in FetchSingleObject<T>");
            }
        }

        public Task<T> Run<T>(ISingleObjectQuery<T> queryObject)
        {
            return Run<T>(DatumConverterFactory, queryObject);
        }

        public Task<DmlResponse> Run(IDatumConverterFactory datumConverterFactory, IDmlQuery queryObject)
        {
            return Run<DmlResponse>(datumConverterFactory, queryObject);
        }

        public Task<DmlResponse> Run(IDmlQuery queryObject)
        {
            return Run(DatumConverterFactory, queryObject);
        }

        public IAsyncEnumerator<T> Run<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
        {
            return new QueryEnumerator<T>(this, datumConverterFactory, queryObject);
        }

        public IAsyncEnumerator<T> Run<T>(ISequenceQuery<T> queryObject)
        {
            return Run(DatumConverterFactory, queryObject);
        }

        public async Task<DmlResponse> Run<T>(IDatumConverterFactory datumConverterFactory, IWriteQuery<T> queryObject)
        {
            var query = new Spec.Query();
            query.token = GetNextToken();
            query.type = Spec.Query.QueryType.START;
            query.query = queryObject.GenerateTerm(datumConverterFactory);

            var response = await InternalRunQuery(query);

            switch (response.type)
            {
                case Response.ResponseType.SUCCESS_SEQUENCE:
                case Response.ResponseType.SUCCESS_ATOM:
                    if (response.response.Count != 1)
                        throw new InvalidOperationException(String.Format("Expected 1 object, received {0}", response.response.Count));
                    return datumConverterFactory.Get<DmlResponse>().ConvertDatum(response.response[0]);
                case Response.ResponseType.CLIENT_ERROR:
                case Response.ResponseType.COMPILE_ERROR:
                case Response.ResponseType.RUNTIME_ERROR:
                    // FIXME: more robust error handling
                    throw new Exception("Error: " + response.response[0].r_str);
                default:
                    throw new InvalidOperationException("Unhandled response type in FetchSingleObject<T>");
            }
        }

        public Task<DmlResponse> Run<T>(IWriteQuery<T> queryObject)
        {
            return Run(DatumConverterFactory, queryObject);
        }

        private class QueryEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly Connection connection;
            private readonly IDatumConverterFactory datumConverterFactory;
            private readonly IDatumConverter<T> datumConverter;
            private readonly ISequenceQuery<T> queryObject;

            private Spec.Query query = null;
            private Response lastResponse = null;
            private int lastResponseIndex = 0;

            public QueryEnumerator(Connection connection, IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
            {
                this.connection = connection;
                this.datumConverterFactory = datumConverterFactory;
                this.datumConverter = datumConverterFactory.Get<T>();
                this.queryObject = queryObject;
            }

            public T Current
            {
                get
                {
                    if (lastResponse == null || lastResponseIndex == -1)
                        throw new InvalidOperationException("Call MoveNext first");
                    else if (lastResponseIndex >= lastResponse.response.Count)
                        throw new InvalidOperationException("You moved past the end of the enumerator");
                    else
                        return datumConverter.ConvertDatum(lastResponse.response[lastResponseIndex]);
                }
            }

            private async Task ReissueQuery()
            {
                lastResponse = await connection.InternalRunQuery(query);
                lastResponseIndex = -1;

                if (lastResponse.type != Response.ResponseType.SUCCESS_SEQUENCE &&
                    lastResponse.type != Response.ResponseType.SUCCESS_PARTIAL)
                {
                    throw new Exception("Unexpected response type to query: " + lastResponse.type);
                }
            }

            public async Task<bool> MoveNext()
            {
                if (lastResponse == null)
                {
                    query = new Spec.Query();
                    query.token = connection.GetNextToken();
                    query.type = Spec.Query.QueryType.START;
                    query.query = this.queryObject.GenerateTerm(datumConverterFactory);
                    await ReissueQuery();
                }

                if (lastResponseIndex < (lastResponse.response.Count - 1))
                {
                    lastResponseIndex += 1;
                    return true;
                }

                if (lastResponse.type == Response.ResponseType.SUCCESS_SEQUENCE)
                {
                    return false;
                }
                else if (lastResponse.type == Response.ResponseType.SUCCESS_PARTIAL)
                {
                    query.type = RethinkDb.Spec.Query.QueryType.CONTINUE;
                    query.query = null;
                    await ReissueQuery();
                    return await MoveNext();
                }
                else
                {
                    throw new InvalidOperationException("Unreachable code; ReissueQuery should prevent reaching this condition");
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Logger.Debug("Disposing Connection");
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
