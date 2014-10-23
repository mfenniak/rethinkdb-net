using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;
using RethinkDb.Logging;
using SineSignal.Ottoman.Serialization;

namespace RethinkDb
{
    public sealed class Connection : IConnectableConnection, IDisposable
    {
        private static byte[] connectHeader = null;
        private static byte[] protocolHeader = null;

        private TcpClient tcpClient;
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
                UriDatumConverterFactory.Instance,
                TupleDatumConverterFactory.Instance,
                ArrayDatumConverterFactory.Instance,
                AnonymousTypeDatumConverterFactory.Instance,
                EnumDatumConverterFactory.Instance,
                NullableDatumConverterFactory.Instance,
                ListDatumConverterFactory.Instance,
                TimeSpanDatumConverterFactory.Instance,
                GroupingDictionaryDatumConverterFactory.Instance
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

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
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

            TcpClient tcpClient = null;
            NetworkStream stream = null;

            try
            {
                Action abortToken = () => {
                    Logger.Warning("Timeout when connecting to endpoint {0}", endpoint);
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
                        stream = null;
                    }
                    if (tcpClient != null)
                    {
                        try
                        {
                            tcpClient.Close();
                            ((IDisposable)tcpClient).Dispose();
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug("Exception occurred while disposing network socket during exception handling: {0}; probably not important", ex);
                        }
                        tcpClient = null;
                    }
                };
                using (cancellationToken.Register(abortToken))
                {
                    tcpClient = new TcpClient();
                    await tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);
                }

                Logger.Debug("Connected to {0}", endpoint);

                if (connectHeader == null)
                {
                    var header = BitConverter.GetBytes((int)Spec.VersionDummy.Version.V0_3);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(header, 0, header.Length);
                    connectHeader = header;
                }

                stream = tcpClient.GetStream();
                this.tcpClient = tcpClient;
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

                if (protocolHeader == null)
                {
                    var header = BitConverter.GetBytes((int)Spec.VersionDummy.Protocol.JSON);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(header, 0, header.Length);
                    protocolHeader = header;
                }
                await stream.WriteAsync(protocolHeader, 0, protocolHeader.Length, cancellationToken);

                byte[] authReponseBuffer = new byte[1024];
                var authResponseLength = await ReadUntilNullTerminator(authReponseBuffer, cancellationToken);
                var authResponse = Encoding.ASCII.GetString(authReponseBuffer, 0, authResponseLength);
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
                    stream = null;
                }
                if (tcpClient != null)
                {
                    try
                    {
                        tcpClient.Close();
                        ((IDisposable)tcpClient).Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Exception occurred while disposing network socket during exception handling: {0}; probably not important", ex);
                    }
                    tcpClient = null;
                }
                if (networkException)
                    throw new RethinkDbNetworkException(e.Message, e);
                else
                    throw new RethinkDbInternalErrorException("Unexpected exception: " + e.Message, e);
            }
        }

        private Response ReadJsonResponse(JsonReader json, long token)
        {
            Spec.Response retval = new Spec.Response();
            retval.token = token;

            if (!json.Read() || json.CurrentToken != JsonToken.ObjectStart)
                throw new RethinkDbInternalErrorException("Expected a readable JSON object in response");

            while (true)
            {
                if (!json.Read())
                    throw new RethinkDbInternalErrorException("Unexpected end-of-frame reading JSON response");
                if (json.CurrentToken == JsonToken.ObjectEnd)
                    break;
                if (json.CurrentToken != JsonToken.MemberName)
                    throw new RethinkDbInternalErrorException("Unexpected JSON state");

                string property = (string)json.CurrentTokenValue;
                if (property == "t")
                    retval.type = ReadResponseType(json);
                else if (property == "r")
                    retval.response.AddRange(ReadDatumArray(json));
                else if (property == "b")
                    retval.backtrace = ReadBacktrace(json);
                else if (property == "p")
                    Logger.Warning("Profiling is not currently supported by rethinkdb-net; profiling data will be discarded");
                else
                    Logger.Information("Unexpected property {0} in JSON response; ignoring", property);
            }

            return retval;
        }

        private static Response.ResponseType ReadResponseType(JsonReader json)
        {
            if (!json.Read())
                throw new RethinkDbInternalErrorException("Unexpected end-of-frame reading JSON response");
            if (json.CurrentToken != JsonToken.Int)
                throw new RethinkDbInternalErrorException("Unexpected value in 'r' key in response");
            return (Response.ResponseType)(json.CurrentTokenValue);
        }

        private static IEnumerable<Datum> ReadDatumArray(JsonReader json)
        {
            if (!json.Read())
                throw new RethinkDbInternalErrorException("Unexpected end-of-frame reading JSON response");
            if (json.CurrentToken != JsonToken.ArrayStart)
                throw new RethinkDbInternalErrorException("Unexpected value in 'r' key in response");

            while (true)
            {
                var datum = ReadDatum(json);
                if (datum == null)
                    break;
                yield return datum;
            }
        }

        private static Datum ReadDatum(JsonReader json)
        {
            if (!json.Read())
                throw new RethinkDbInternalErrorException("Unexpected end-of-frame reading JSON response");
            switch (json.CurrentToken)
            {
                case JsonToken.Null:
                    return new Datum() { type = Datum.DatumType.R_NULL };
                case JsonToken.Boolean:
                    return new Datum() { type = Datum.DatumType.R_BOOL, r_bool = (bool)json.CurrentTokenValue };
                case JsonToken.Double:
                    return new Datum() { type = Datum.DatumType.R_NUM, r_num = (double)json.CurrentTokenValue };
                case JsonToken.Int:
                    return new Datum() { type = Datum.DatumType.R_NUM, r_num = (int)json.CurrentTokenValue };
                case JsonToken.Long:
                    return new Datum() { type = Datum.DatumType.R_NUM, r_num = (long)json.CurrentTokenValue };
                case JsonToken.String:
                    return new Datum() { type = Datum.DatumType.R_STR, r_str = (string)json.CurrentTokenValue };
                case JsonToken.ArrayStart:
                    {
                        var retval = new Datum() { type = Datum.DatumType.R_ARRAY };
                        while (true)
                        {
                            var datum = ReadDatum(json);
                            if (datum == null)
                                break;
                            retval.r_array.Add(datum);
                        }
                        return retval;
                    }
                case JsonToken.ObjectStart:
                    {
                        var retval = new Datum() { type = Datum.DatumType.R_OBJECT };
                        while (true)
                        {
                            if (!json.Read())
                                throw new RethinkDbInternalErrorException("Unexpected end-of-frame reading JSON response");
                            if (json.CurrentToken == JsonToken.ObjectEnd)
                                break;
                            if (json.CurrentToken != JsonToken.MemberName)
                                throw new RethinkDbInternalErrorException("Expected MemberName next in object");

                            string memberName = (string)json.CurrentTokenValue;
                            var datum = ReadDatum(json);
                            if (datum == null)
                                throw new RethinkDbInternalErrorException("Expected a datum to be following MemberName");
                            retval.r_object.Add(new Datum.AssocPair() { key = memberName, val = datum });
                        }
                        return retval;
                    }
                case JsonToken.ArrayEnd:
                    // This wouldn't be expected at a datum, but instead signals the end of the array or object
                    // being read by the caller.
                    return null;
                default:
                    throw new RethinkDbInternalErrorException(String.Format("Unexpected token {0} in datum", json.CurrentToken));
            }
        }

        private static Backtrace ReadBacktrace(JsonReader json)
        {
            if (!json.Read())
                throw new RethinkDbInternalErrorException("Unexpected end-of-frame reading JSON response");
            if (json.CurrentToken != JsonToken.ArrayStart)
                throw new RethinkDbInternalErrorException("Unexpected value in 'r' key in response");
            var retval = new Backtrace();
            while (true)
            {
                // FIXME: read backtraces...
                json.Read();
                if (json.CurrentToken == JsonToken.ArrayEnd)
                    break;
            }
            return retval;
        }

        private async Task ReadLoop()
        {
            RethinkDbException responseException = null;
            try
            {
                while (true)
                {
                    byte[] tokenHeader = new byte[8];
                    await ReadMyBytes(tokenHeader);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(tokenHeader, 0, tokenHeader.Length);
                    var token = BitConverter.ToInt64(tokenHeader, 0);
                    Logger.Debug("Received packet token, token is {0}", token);

                    byte[] lengthHeader = new byte[4];
                    await ReadMyBytes(lengthHeader);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(lengthHeader, 0, lengthHeader.Length);
                    var respSize = BitConverter.ToInt32(lengthHeader, 0);
                    Logger.Debug("Received packet size header, packet is {0} bytes", respSize);

                    byte[] retVal = new byte[respSize];
                    await ReadMyBytes(retVal);
                    Logger.Debug("Received packet completely");
                    using (var memoryBuffer = new MemoryStream(retVal))
                    using (var textReader = new StreamReader(memoryBuffer))
                    {
                        var json = new JsonReader(textReader);
                        var response = ReadJsonResponse(json, token);

                        //var response = Serializer.Deserialize<Response>(memoryBuffer);
                        Logger.Debug("Received packet deserialized to response for query token {0}, type: {1}", response.token, response.type);

                        /*
                        if (response.token != token)
                            // I'm assuming this isn't possible, but just testing here; this goes away with the JSON
                            // protocol conversion, so I don't have to worry about the exception type.
                            throw new InvalidOperationException("response.token != token");
                        */

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
            catch (RethinkDbNetworkException e)
            {
                // Network exception?  That's basically our "graceful" termination of ReadLoop.
                Logger.Debug("ReadLoop terminated by network exception; this is expected if the connection was closed intentionally");
                responseException = new RethinkDbNetworkException("ReadLoop terminated unexpectedly while waiting for response from query", e);
            }
            catch (ObjectDisposedException e)
            {
                // Probably a result of this connection being disposed; that's pretty graceful too...
                Logger.Debug("ReadLoop terminated by network exception; this is expected if the connection was closed intentionally");
                responseException = new RethinkDbRuntimeException("ReadLoop terminated unexpectedly due to connection Dispose while waiting for response from query", e);
            }
            catch (Exception e)
            {
                Logger.Fatal("Exception occurred in Connection.ReadLoop: {0}; this connection will no longer function correctly, no error recovery will take place", e);
                responseException = new RethinkDbInternalErrorException("Unexpected exception in ReadLoop prevented this query from returning data", e);
            }

            var responseSnapshot = tokenResponse.ToList();
            if (responseSnapshot.Count > 0)
                Logger.Warning("{0} queries were still waiting for responses from a connection that is closing; they'll receive exceptions instead", responseSnapshot.Count);
            foreach (var kvp in responseSnapshot)
            {
                kvp.Value.SetException(responseException);
                tokenResponse.Remove(kvp.Key);
            }
            if (tokenResponse.Count != 0)
                Logger.Warning("Cleanup in ReadLoop termination didn't succeed, there are still tasks waiting for data from this connection that will never receive it");
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

        private static void WriteQuery(JsonWriter writer, Spec.Query query)
        {
            writer.BeginArray();
            writer.WriteNumber((int)query.type);
            if (query.type == Spec.Query.QueryType.START)
            {
                WriteTerm(writer, query.query);
                writer.BeginObject();
                foreach (var opt in query.global_optargs)
                {
                    writer.WriteMember(opt.key);
                    WriteTerm(writer, opt.val);
                }
                writer.EndObject();
            }
            writer.EndArray();
        }

        private static void WriteTerm(JsonWriter writer, Spec.Term term)
        {
            if (term.type == Term.TermType.DATUM)
            {
                WriteDatum(writer, term.datum);
                return;
            }

            writer.BeginArray();
            writer.WriteNumber((int)term.type);
            if (term.args.Count > 0)
            {
                writer.BeginArray();
                foreach (var arg in term.args)
                {
                    WriteTerm(writer, arg);
                }
                writer.EndArray();
            }
            if (term.optargs.Count > 0)
            {
                writer.BeginObject();
                foreach (var opt in term.optargs)
                {
                    writer.WriteMember(opt.key);
                    WriteTerm(writer, opt.val);
                }
                writer.EndObject();
            }
            writer.EndArray();
        }

        private static void WriteDatum(JsonWriter writer, Spec.Datum datum)
        {
            switch (datum.type)
            {
                case Spec.Datum.DatumType.R_ARRAY:
                    writer.BeginArray();
                    foreach (var innerDatum in datum.r_array)
                        WriteDatum(writer, innerDatum);
                    writer.EndArray();
                    break;
                case Spec.Datum.DatumType.R_BOOL:
                    writer.WriteBoolean(datum.r_bool);
                    break;
                case Spec.Datum.DatumType.R_JSON:
                    throw new NotSupportedException();
                case Spec.Datum.DatumType.R_NULL:
                    writer.WriteNull();
                    break;
                case Spec.Datum.DatumType.R_NUM:
                    writer.WriteNumber(datum.r_num);
                    break;
                case Spec.Datum.DatumType.R_OBJECT:
                    writer.BeginObject();
                    foreach (var kvp in datum.r_object)
                    {
                        writer.WriteMember(kvp.key);
                        WriteDatum(writer, kvp.val);
                    }
                    writer.EndObject();
                    break;
                case Spec.Datum.DatumType.R_STR:
                    writer.WriteString(datum.r_str);
                    break;
            }
        }

        internal async Task<Response> InternalRunQuery(Spec.Query query, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<Response>();
            tokenResponse[query.token] = tcs;

            Action abortToken = () => {
                Logger.Warning("Query token {0} timed out after {1}", query.token, this.QueryTimeout);
                if (tokenResponse.Remove(query.token))
                    tcs.SetCanceled();
            };
            using (cancellationToken.Register(abortToken))
            {
                using (var memoryBuffer = new MemoryStream(1024))
                {
                    // FIXME: don't create an encoder every time
                    // and also, figure out exactly the right encoding that rethinkdb is expecting
                    using (var streamWriter = new StreamWriter(memoryBuffer, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                    {
                        WriteQuery(new JsonWriter(streamWriter), query);
                    }

                    var data = memoryBuffer.ToArray();
                    string dataStr = Encoding.UTF8.GetString(data);
                    Logger.Information("JSON query: {0}", dataStr);
                    var lengthHeader = BitConverter.GetBytes(data.Length);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(lengthHeader, 0, lengthHeader.Length);

                    var tokenHeader = BitConverter.GetBytes(query.token);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(tokenHeader, 0, tokenHeader.Length);

                    // Put query.token into writeTokenLock if writeTokenLock is 0 (ie. unlocked).  If it's not 0,
                    // spin-lock on the compare exchange.
                    while (Interlocked.CompareExchange(ref writeTokenLock, query.token, 0) != 0)
                        ;

                    try
                    {
                        Logger.Debug("Writing packet, {0} bytes", data.Length);
                        await stream.WriteAsync(tokenHeader, 0, tokenHeader.Length, cancellationToken);
                        await stream.WriteAsync(lengthHeader, 0, lengthHeader.Length, cancellationToken);
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

        public async Task<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, IScalarQuery<T> queryObject, CancellationToken cancellationToken)
        {
            var query = new Spec.Query();
            query.token = GetNextToken();
            query.type = Spec.Query.QueryType.START;
            query.query = queryObject.GenerateTerm(datumConverterFactory);

            var response = await InternalRunQuery(query, cancellationToken);

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

        public IAsyncEnumerator<T> RunAsync<T>(IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
        {
            return new QueryEnumerator<T>(this, datumConverterFactory, queryObject);
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

            public IConnection Connection
            {
                get { return connection; }
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

            public async Task Dispose(CancellationToken cancellationToken)
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
                var response = await connection.InternalRunQuery(query, cancellationToken);

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

            private async Task ReissueQuery(CancellationToken cancellationToken)
            {
                lastResponse = await connection.InternalRunQuery(query, cancellationToken);
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

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                if (disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (lastResponse == null)
                {
                    query = new Spec.Query();
                    query.token = connection.GetNextToken();
                    query.type = Spec.Query.QueryType.START;
                    query.query = this.queryObject.GenerateTerm(datumConverterFactory);
                    await ReissueQuery(cancellationToken);
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
                    await ReissueQuery(cancellationToken);
                    return await MoveNext(cancellationToken);
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
            if (tcpClient != null)
            {
                try
                {
                    tcpClient.Close();
                    ((IDisposable)tcpClient).Dispose();
                }
                catch (Exception)
                {
                }
                tcpClient = null;
            }
        }

        #endregion
    }
}
