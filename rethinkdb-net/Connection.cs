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
using System.Diagnostics;
using System.Text;

namespace RethinkDb
{
    public sealed class Connection : IConnection, IDisposable
    {
        private static TaskFactory taskFactory = new TaskFactory();
        private static byte[] connectHeader = null;
       
        private Socket socket;
        private NetworkStream stream;
        private long nextToken = 1;
        private long writeTokenLock = 0;
        private IDictionary<long, TaskCompletionSource<Response>> tokenResponse = new ConcurrentDictionary<long, TaskCompletionSource<Response>>();

        public Connection()
        {
            DatumConverterFactory = new AggregateDatumConverterFactory(
                PrimitiveDatumConverterFactory.Instance,
                DataContractDatumConverterFactory.Instance,
                DateTimeDatumConverterFactory.Instance,
                DateTimeOffsetDatumConverterFactory.Instance,
                GuidDatumConverterFactory.Instance,
                TupleDatumConverterFactory.Instance,
                ArrayDatumConverterFactory.Instance,
                AnonymousTypeDatumConverterFactory.Instance,
                NullableDatumConverterFactory.Instance
            );
            ConnectTimeout = QueryTimeout = TimeSpan.FromSeconds(30);
        }

        public Connection(params EndPoint[] endPoints)
            : this()
        {
            this.EndPoints = endPoints;
        }

        public IEnumerable<EndPoint> EndPoints
        {
            get;
            set;
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

        public TimeSpan ConnectTimeout
        {
            get;
            set;
        }       

        public TimeSpan QueryTimeout
        {
            get;
            set;
        }

        public string AuthorizationKey
        {
            get;
            set;
        }

        public async Task ConnectAsync()
        {
            var cancellationToken = new CancellationTokenSource(this.ConnectTimeout).Token;

            foreach (var ep in EndPoints)
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
                            Logger.Debug("DNS lookup {0} into: [{1}]", dnsEndpoint.Host, resolvedIpEndpoints.EnumerableToString());
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

            Logger.Error("Failed to resolve or connect to any provided address; connection failed");
            throw new RethinkDbNetworkException("Failed to resolve a connectable address.");
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
                    var header = BitConverter.GetBytes((int)Spec.VersionDummy.Version.V0_2);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(header, 0, header.Length);
                    connectHeader = header;
                }

                stream = new NetworkStream(socket, true);
                this.socket = socket;
                this.stream = stream;

                await stream.WriteAsync(connectHeader, 0, connectHeader.Length, cancellationToken);
                Logger.Debug("Sent ReQL header");

                if (String.IsNullOrEmpty(AuthorizationKey))
                {
                    await stream.WriteAsync(new byte[] { 0, 0, 0, 0 }, 0, 4, cancellationToken);
                }
                else
                {
                    var keyInBytes = Encoding.UTF8.GetBytes(AuthorizationKey);
                    var authKeyLength = BitConverter.GetBytes(keyInBytes.Length);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(authKeyLength, 0, authKeyLength.Length);
                    await stream.WriteAsync(authKeyLength, 0, authKeyLength.Length);
                    await stream.WriteAsync(keyInBytes, 0, keyInBytes.Length);
                }

                byte[] authReponseBuffer = new byte[1024];
                var authResponseLength = await ReadUntilNullTerminator(authReponseBuffer, cancellationToken);
                var authResponse = Encoding.UTF8.GetString(authReponseBuffer, 0, authResponseLength);
                if (authResponse != "SUCCESS")
                    throw new RethinkDbRuntimeException("Unexpected authentication response; expected SUCCESS but got: " + authResponse);

#pragma warning disable 4014
                ReadLoop();
#pragma warning restore 4014
            }
            catch (Exception e)
            {
                bool networkException = (e is SocketException || e is IOException);
                if (networkException)
                    Logger.Error("Network related exception occurred while connecting to endpoint {0}: {1}; tearing down connection", endpoint, e);
                else
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
                if (networkException)
                    throw new RethinkDbNetworkException(e.Message, e);
                else
                    throw new RethinkDbInternalErrorException("Unexpected exception: " + e.Message, e);
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
                    Logger.Debug("Received packet completely");
                    using (var memoryBuffer = new MemoryStream(retVal))
                    {
                        var response = Serializer.Deserialize<Response>(memoryBuffer);
                        Logger.Debug("Received packet deserialized to response for query token {0}, type: {1}", response.token, response.type);

                        TaskCompletionSource<Response> tcs;
                        if (tokenResponse.TryGetValue(response.token, out tcs))
                        {
                            tokenResponse.Remove(response.token);
                            // Send the result to the waiting thread via the thread-pool.  If we invoke
                            // tcs.SetResult(response) synchronously, we actually block until the response handling
                            // is completed, which could be an unknown quantity of user code.  It could also cause
                            // a deadlock (see mfenniak/rethinkdb-net#112).
                            ThreadPool.QueueUserWorkItem(_ => tcs.SetResult(response));
                        }
                        else
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

        private async Task<int> ReadUntilNullTerminator(byte[] buffer, CancellationToken cancellationToken)
        {
            int totalBytesRead = 0;
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead, cancellationToken);
                totalBytesRead += bytesRead;
                Logger.Debug("Received {0} / {1} bytes in NullTerminator buffer", bytesRead, buffer.Length);

                if (bytesRead == 0)
                    throw new RethinkDbNetworkException("Network stream closed while attempting to read");
                else if (buffer[totalBytesRead - 1] == 0)
                    return totalBytesRead - 1;
                else if (totalBytesRead == buffer.Length)
                    throw new RethinkDbNetworkException("Ran out of space in buffer while looking for a null-terminated string");
            }
        }

        private async Task ReadMyBytes(byte[] buffer)
        {
            int totalBytesRead = 0;
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead);
                totalBytesRead += bytesRead;
                Logger.Debug("Received {0} / {1} bytes of packet", bytesRead, buffer.Length);

                if (bytesRead == 0)
                    throw new RethinkDbNetworkException("Network stream closed while attempting to read");
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

            var cancellationToken = new CancellationTokenSource(this.QueryTimeout).Token;
            Action abortToken = () => {
                Logger.Warning("Query token {0} timed out after {1}", query.token, this.QueryTimeout);
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

        public async Task<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject)
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
                        throw new RethinkDbRuntimeException(String.Format("Expected 1 object, received {0}", response.response.Count));
                    return datumConverterFactory.Get<T>().ConvertDatum(response.response[0]);
                case Response.ResponseType.CLIENT_ERROR:
                case Response.ResponseType.COMPILE_ERROR:
                    throw new RethinkDbInternalErrorException("Client error: " + response.response[0].r_str);
                case Response.ResponseType.RUNTIME_ERROR:
                    throw new RethinkDbRuntimeException("Runtime error: " + response.response[0].r_str);
                default:
                    throw new RethinkDbInternalErrorException("Unhandled response type: " + response.type);
            }
        }

        public Task<T> RunAsync<T>(ISingleObjectQuery<T> queryObject)
        {
            return RunAsync<T>(DatumConverterFactory, queryObject);
        }

        public Task<DmlResponse> RunAsync(IDatumConverterFactory datumConverterFactory, IDmlQuery queryObject)
        {
            return RunAsync<DmlResponse>(datumConverterFactory, queryObject);
        }

        public Task<DmlResponse> RunAsync(IDmlQuery queryObject)
        {
            return RunAsync(DatumConverterFactory, queryObject);
        }

        public IAsyncEnumerator<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
        {
            return new QueryEnumerator<T>(this, datumConverterFactory, queryObject);
        }

        public IAsyncEnumerator<T> RunAsync<T>(ISequenceQuery<T> queryObject)
        {
            return RunAsync(DatumConverterFactory, queryObject);
        }

        public async Task<DmlResponse> RunAsync<T>(IDatumConverterFactory datumConverterFactory, IWriteQuery<T> queryObject)
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
                        throw new RethinkDbRuntimeException(String.Format("Expected 1 object, received {0}", response.response.Count));
                    return datumConverterFactory.Get<DmlResponse>().ConvertDatum(response.response[0]);
                case Response.ResponseType.CLIENT_ERROR:
                case Response.ResponseType.COMPILE_ERROR:
                    throw new RethinkDbInternalErrorException("Client error: " + response.response[0].r_str);
                case Response.ResponseType.RUNTIME_ERROR:
                    throw new RethinkDbRuntimeException("Runtime error: " + response.response[0].r_str);
                default:
                    throw new RethinkDbInternalErrorException("Unhandled response type: " + response.type);
            }
        }

        public Task<DmlResponse> RunAsync<T>(IWriteQuery<T> queryObject)
        {
            return RunAsync(DatumConverterFactory, queryObject);
        }

        private class QueryEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly Connection connection;
            private readonly IDatumConverterFactory datumConverterFactory;
            private readonly IDatumConverter<T> datumConverter;
            private readonly ISequenceQuery<T> queryObject;
            private readonly StackTrace stackTrace;

            private Spec.Query query = null;
            private Response lastResponse = null;
            private int lastResponseIndex = 0;
            private bool disposed = false;

            public QueryEnumerator(Connection connection, IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
            {
                this.connection = connection;
                this.datumConverterFactory = datumConverterFactory;
                this.datumConverter = datumConverterFactory.Get<T>();
                this.queryObject = queryObject;
                this.stackTrace = new StackTrace(true);
            }

            ~QueryEnumerator()
            {
                if (!disposed)
                {
                    var c = connection;
                    if (c == null)
                        return;

                    var l = c.Logger;
                    if (l != null)
                        return;

                    l.Warning("QueryEnumerator finalizer was called and object was not disposed; originally created at: {0}", stackTrace);
                }
            }

            public async Task Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                GC.SuppressFinalize(this);

                if (query == null)
                    return;
                if (lastResponse == null)
                    return;
                if (lastResponse.type != Response.ResponseType.SUCCESS_PARTIAL)
                    return;

                // Looks like we have a query in-progress that we should stop on the server-side to free up resources.
                query.type = Spec.Query.QueryType.STOP;
                query.query = null;
                var response = await connection.InternalRunQuery(query);

                switch (response.type)
                {
                    case Response.ResponseType.SUCCESS_ATOM:
                        break;
                    case Response.ResponseType.CLIENT_ERROR:
                    case Response.ResponseType.COMPILE_ERROR:
                        throw new RethinkDbInternalErrorException("Client error: " + response.response[0].r_str);
                    case Response.ResponseType.RUNTIME_ERROR:
                        throw new RethinkDbRuntimeException("Runtime error: " + response.response[0].r_str);
                    default:
                        throw new RethinkDbInternalErrorException("Unhandled response type: " + response.type);
                }
            }

            private int LoadedRecordCount()
            {
                if (lastResponse.type == Response.ResponseType.SUCCESS_ATOM)
                    return lastResponse.response[0].r_array.Count;
                else
                    return lastResponse.response.Count;
            }

            public T Current
            {
                get
                {
                    if (disposed)
                        throw new ObjectDisposedException(GetType().FullName);
                    if (lastResponse == null || lastResponseIndex == -1)
                        throw new InvalidOperationException("Call MoveNext first");
                    else if (lastResponseIndex >= LoadedRecordCount())
                        throw new InvalidOperationException("You moved past the end of the enumerator");
                    else if (lastResponse.type == Response.ResponseType.SUCCESS_ATOM)
                        return datumConverter.ConvertDatum(lastResponse.response[0].r_array[lastResponseIndex]);
                    else
                        return datumConverter.ConvertDatum(lastResponse.response[lastResponseIndex]);
                }
            }

            private async Task ReissueQuery()
            {
                lastResponse = await connection.InternalRunQuery(query);
                lastResponseIndex = -1;

                switch (lastResponse.type)
                {
                    case Response.ResponseType.SUCCESS_SEQUENCE:
                    case Response.ResponseType.SUCCESS_PARTIAL:
                        break;
                    case Response.ResponseType.SUCCESS_ATOM:
                        if (lastResponse.response[0].type != Datum.DatumType.R_ARRAY)
                            throw new RethinkDbRuntimeException("Received an unexpected non-enumerable response to an enumeration query");
                        break;
                    case Response.ResponseType.CLIENT_ERROR:
                    case Response.ResponseType.COMPILE_ERROR:
                        throw new RethinkDbInternalErrorException("Client error: " + lastResponse.response[0].r_str);
                    case Response.ResponseType.RUNTIME_ERROR:
                        throw new RethinkDbRuntimeException("Runtime error: " + lastResponse.response[0].r_str);
                    default:
                        throw new RethinkDbInternalErrorException("Unhandled response type: " + lastResponse.type);
                }
            }

            public async Task<bool> MoveNext()
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (lastResponse == null)
                {
                    query = new Spec.Query();
                    query.token = connection.GetNextToken();
                    query.type = Spec.Query.QueryType.START;
                    query.query = this.queryObject.GenerateTerm(datumConverterFactory);
                    await ReissueQuery();
                }

                if (lastResponseIndex < (LoadedRecordCount() - 1))
                {
                    lastResponseIndex += 1;
                    return true;
                }

                if (lastResponse.type == Response.ResponseType.SUCCESS_SEQUENCE ||
                    lastResponse.type == Response.ResponseType.SUCCESS_ATOM)
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
                    throw new RethinkDbInternalErrorException("Unreachable code; ReissueQuery should prevent reaching this condition");
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
        #region IConnection implementation

        public void Connect()
        {
            ConnectAsync().Wait();
        }

        public T Run<T>(IDatumConverterFactory datumConverterFactory, ISingleObjectQuery<T> queryObject)
        {
            return RunAsync(datumConverterFactory, queryObject).Result;
        }

        public T Run<T>(ISingleObjectQuery<T> queryObject)
        {
            return RunAsync(queryObject).Result;
        }

        public IEnumerable<T> Run<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
        {
            return new AsyncEnumerableSynchronizer<T>(() => RunAsync(datumConverterFactory, queryObject));
        }

        public IEnumerable<T> Run<T>(ISequenceQuery<T> queryObject)
        {
            return new AsyncEnumerableSynchronizer<T>(() => RunAsync(queryObject));
        }

        public DmlResponse Run(IDatumConverterFactory datumConverterFactory, IDmlQuery queryObject)
        {
            return RunAsync(datumConverterFactory, queryObject).Result;
        }

        public DmlResponse Run(IDmlQuery queryObject)
        {
            return RunAsync(queryObject).Result;
        }

        public DmlResponse Run<T>(IDatumConverterFactory datumConverterFactory, IWriteQuery<T> queryObject)
        {
            return RunAsync(datumConverterFactory, queryObject).Result;
        }

        public DmlResponse Run<T>(IWriteQuery<T> queryObject)
        {
            return RunAsync(queryObject).Result;
        }

        private class AsyncEnumerableSynchronizer<T> : IEnumerable<T>
        {
            private readonly Func<IAsyncEnumerator<T>> asyncEnumeratorFactory;

            public AsyncEnumerableSynchronizer(Func<IAsyncEnumerator<T>> asyncEnumeratorFactory)
            {
                this.asyncEnumeratorFactory = asyncEnumeratorFactory;
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new AsyncEnumeratorSynchronizer<T>(asyncEnumeratorFactory());
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new AsyncEnumeratorSynchronizer<T>(asyncEnumeratorFactory());
            }
        }

        private sealed class AsyncEnumeratorSynchronizer<T> : IEnumerator<T>
        {
            private IAsyncEnumerator<T> asyncEnumerator;

            public AsyncEnumeratorSynchronizer(IAsyncEnumerator<T> asyncEnumerator)
            {
                this.asyncEnumerator = asyncEnumerator;
            }

            #region IDisposable implementation

            public void Dispose()
            {
                if (this.asyncEnumerator != null)
                {
                    this.asyncEnumerator.Dispose().Wait();
                    this.asyncEnumerator = null;
                }
            }

            #endregion
            #region IEnumerator implementation

            public bool MoveNext()
            {
                if (asyncEnumerator == null)
                    throw new ObjectDisposedException(GetType().FullName);
                return asyncEnumerator.MoveNext().Result;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public object Current
            {
                get
                {
                    if (asyncEnumerator == null)
                        throw new ObjectDisposedException(GetType().FullName);
                    return asyncEnumerator.Current;
                }
            }

            #endregion
            #region IEnumerator implementation

            T IEnumerator<T>.Current
            {
                get
                {
                    if (asyncEnumerator == null)
                        throw new ObjectDisposedException(GetType().FullName);
                    return asyncEnumerator.Current;
                }
            }

            #endregion
        }

        #endregion
    }
}
