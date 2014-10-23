using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using SineSignal.Ottoman.Serialization;
using System.Threading;
using RethinkDb.Logging;
using RethinkDb.Spec;
using System.Linq;
using System.Collections.Generic;

namespace RethinkDb.Protocols
{
    [ImmutableObject(true)]
    public class Version_0_3_Json : Version_0_3
    {
        public static readonly Version_0_3_Json Instance = new Version_0_3_Json();

        private byte[] protocolHeader;

        private Version_0_3_Json()
        {
            var header = BitConverter.GetBytes((int)Spec.VersionDummy.Protocol.JSON);
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
                // FIXME: don't create an encoder every time
                // and also, figure out exactly the right encoding that rethinkdb is expecting
                using (var streamWriter = new StreamWriter(memoryBuffer, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                {
                    WriteQuery(new JsonWriter(streamWriter), query);
                }

                var data = memoryBuffer.ToArray();
                string dataStr = Encoding.UTF8.GetString(data);
                logger.Information("JSON query: {0}", dataStr);
                var lengthHeader = BitConverter.GetBytes(data.Length);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(lengthHeader, 0, lengthHeader.Length);

                var tokenHeader = BitConverter.GetBytes(query.token);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(tokenHeader, 0, tokenHeader.Length);

                logger.Debug("Writing packet, {0} bytes", data.Length);
                await stream.WriteAsync(tokenHeader, 0, tokenHeader.Length, cancellationToken);
                await stream.WriteAsync(lengthHeader, 0, lengthHeader.Length, cancellationToken);
                await stream.WriteAsync(data, 0, data.Length, cancellationToken);
            }
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
            if (term.args.Count > 0 || term.optargs.Count > 0)
            {
                writer.BeginArray();
                foreach (var arg in term.args)
                {
                    WriteTerm(writer, arg);
                }
                writer.EndArray();
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
                case Spec.Datum.DatumType.R_STR:
                    writer.WriteString(datum.r_str);
                    break;
                case Spec.Datum.DatumType.R_ARRAY:
                    {
                        var newterm = new Term() { type = Term.TermType.MAKE_ARRAY };
                        newterm.args.AddRange(datum.r_array.Select(ap => new Term()
                        {
                            type = Term.TermType.DATUM,
                            datum = ap,
                        }));
                        WriteTerm(writer, newterm);
                    }
                    break;
                case Spec.Datum.DatumType.R_OBJECT:
                    {
                        var newterm = new Term() { type = Term.TermType.MAKE_OBJ };
                        newterm.optargs.AddRange(datum.r_object.Select(ap => new Term.AssocPair()
                        {
                            key = ap.key,
                            val = new Term()
                            {
                                type = Term.TermType.DATUM,
                                datum = ap.val
                            }
                        }));
                        WriteTerm(writer, newterm);
                    }
                    break;
            }
        }

        public override async Task<Response> ReadResponseFromStream(Stream stream, ILogger logger)
        {
            byte[] tokenHeader = new byte[8];
            await stream.ReadMyBytes(logger, tokenHeader);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(tokenHeader, 0, tokenHeader.Length);
            var token = BitConverter.ToInt64(tokenHeader, 0);
            logger.Debug("Received packet token, token is {0}", token);

            byte[] lengthHeader = new byte[4];
            await stream.ReadMyBytes(logger, lengthHeader);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(lengthHeader, 0, lengthHeader.Length);
            var respSize = BitConverter.ToInt32(lengthHeader, 0);
            logger.Debug("Received packet size header, packet is {0} bytes", respSize);

            byte[] retVal = new byte[respSize];
            await stream.ReadMyBytes(logger, retVal);
            logger.Debug("Received packet completely");
            using (var memoryBuffer = new MemoryStream(retVal))
            using (var textReader = new StreamReader(memoryBuffer))
            {
                var json = new JsonReader(textReader);
                var response = ReadJsonResponse(json, token, logger);
                logger.Debug("Received packet deserialized to response for query token {0}, type: {1}", response.token, response.type);
                return response;
            }
        }

        private Response ReadJsonResponse(JsonReader json, long token, ILogger logger)
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
                    logger.Warning("Profiling is not currently supported by rethinkdb-net; profiling data will be discarded");
                else
                    logger.Information("Unexpected property {0} in JSON response; ignoring", property);
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

    }
}
