using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RethinkDb.DatumConverters;
using RethinkDb.Newtonsoft.Converters;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft
{
    public class DatumReader : JsonReader
    {
        private readonly Stack<DatumReaderToken> stack = new Stack<DatumReaderToken>();

        public DatumReader(Datum datum)
        {
            this.stack.Push(new DatumReaderToken(datum));
        }

        private DatumReaderToken Context
        {
            get { return this.stack.Peek(); }
        }

        public override bool Read()
        {
            return this.ReadInternal();
        }

        public override int? ReadAsInt32()
        {
            this.ReadInternal(ReadAs.Int32);
            return null;
        }

        public override string ReadAsString()
        {
            this.ReadInternal();
            return null;
        }

        public override byte[] ReadAsBytes()
        {
            this.ReadInternal(ReadAs.ByteArray);
            return null;
        }

        public override decimal? ReadAsDecimal()
        {
            this.ReadInternal();
            return null;
        }

        public override DateTime? ReadAsDateTime()
        {
            this.ReadInternal(ReadAs.DateTime);
            return null;
        }

        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            this.ReadInternal(ReadAs.DateTimeOffset);
            return null;
        }

        public bool ReadInternal(ReadAs? readAs = null)
        {
            switch (this.CurrentState)
            {
                case State.Start:
                    if (Context.IsObject && readAs == null)
                    {
                        this.SetToken(JsonToken.StartObject);
                        return true;
                    }
                    if (Context.IsArray && readAs == null)
                    {
                        this.SetToken(JsonToken.StartArray);
                        return true;
                    }
                    ReadDatum(Context.Datum, readAs);
                    return true;

                case State.Complete:
                case State.Closed:
                    return false;

                case State.Property:
                {
                    ReadDatum(Context.AssocPairs.Current.val, readAs);
                    return true;
                }

                case State.ObjectStart:
                case State.ArrayStart:
                case State.PostValue:
                    if (this.stack.Count == 0)
                        return false;

                    if (Context.IsArray)
                    {
                        if (Context.Array.MoveNext())
                        {
                            ReadDatum(Context.Array.Current);
                            return true;
                        }

                        //else pop state, no more elements
                        this.stack.Pop();
                        SetToken(JsonToken.EndArray);
                        return true;
                    }

                    if (Context.IsObject && readAs == null)
                    {
                        if (Context.AssocPairs.MoveNext())
                        {
                            SetToken(JsonToken.PropertyName, Context.AssocPairs.Current.key);
                            return true;
                        }

                        //else pop state, no more elements
                        this.stack.Pop();
                        SetToken(JsonToken.EndObject);
                        return true;
                    }

                    return false;
            }
            return false;
        }


        private void ReadDatum(Datum datum, ReadAs? readAs = null)
        {
            if (ReadAsSpecialType(datum, readAs))
            {
                return;
            }
            switch (datum.type)
            {
                case Datum.DatumType.R_NUM:
                    SetToken(JsonToken.Float, datum.r_num);
                    return;
                case Datum.DatumType.R_STR:
                    SetToken(JsonToken.String, datum.r_str);
                    return;
                case Datum.DatumType.R_BOOL:
                    SetToken(JsonToken.Boolean, datum.r_bool);
                    return;
                case Datum.DatumType.R_NULL:
                    SetToken(JsonToken.Null);
                    return;

                    //The datum stores more structure
                case Datum.DatumType.R_ARRAY:
                    SetToken(JsonToken.StartArray);
                    stack.Push(new DatumReaderToken(datum));
                    return;
                case Datum.DatumType.R_OBJECT:
                    SetToken(JsonToken.StartObject);
                    stack.Push(new DatumReaderToken(datum));
                    return;

                default:
                    Demand.Require(true, "Unknown handing datum type {0}.", datum.type);
                    return;
            }
        }

        private bool ReadAsSpecialType(Datum datum, ReadAs? readAs)
        {
            if (readAs == null) return false;
            if (datum.type == Datum.DatumType.R_NULL) return false;

            if (readAs == ReadAs.DateTime)
            {
                // JSON.NET is giving us a hint that it's expecting a DateTime.
                var dateTime = DateTimeDatumConverter.Instance.Value.ConvertDatum(datum);
                this.SetToken(JsonToken.Date, dateTime);
                return true;
            }
            if (readAs == ReadAs.DateTimeOffset)
            {
                // JSON.NET is giving us a hint that it's expecting a DateTimeOffset.
                var dateTimeOffset = DateTimeOffsetDatumConverter.Instance.Value.ConvertDatum(datum);
                this.SetToken(JsonToken.Date, dateTimeOffset);
                return true;
            }
            if (readAs == ReadAs.Int32)
            {
                this.SetToken(JsonToken.Integer, Convert.ToInt32(datum.r_num));
                return true;
            }
            if (readAs == ReadAs.ByteArray)
            {
                var bytes = ArrayDatumConverterFactory.Instance.Get<byte[]>(
                    PrimitiveDatumConverterFactory.Instance
                    ).ConvertDatum(datum);
                this.SetToken(JsonToken.Bytes, bytes);
                return true;
            }

            return false;
        }

        public enum ReadAs
        {
            DateTime,
            DateTimeOffset,
            Int32,
            ByteArray
        }
    }
}