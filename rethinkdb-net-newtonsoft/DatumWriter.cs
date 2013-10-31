using System;
using Newtonsoft.Json;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft
{
    public class DatumWriter : JsonWriter
    {
        private DatumWriterToken root;
        private DatumWriterToken parent;
        private string propertyName;

        public Datum GetRootDatum()
        {
            return root.Datum;
        }

        public override void Flush()
        {
        }

        private void AddParent(DatumWriterToken writerToken)
        {
            AddToken(writerToken);
            this.parent = writerToken;
        }

        private void RemoveParent()
        {
            parent = parent.Parent;
        }

        protected internal void AddValue(Datum d)
        {
            if (parent == null)
            {
                SerializeSingleValue(d);
                return;
            }

            if (parent.Datum.type == Datum.DatumType.R_OBJECT)
                parent.Datum.r_object.Add(new Datum.AssocPair() {key = propertyName, val = d});
            else
                parent.Datum.r_array.Add(d);
        }

        private void SerializeSingleValue(Datum d)
        {
            //No object start or array start has been called.
            //We are only serializing a single value / datum.
            this.root = new DatumWriterToken(d);
            this.parent = this.root;
        }

        private void AddToken(DatumWriterToken writerToken)
        {
            if (parent != null)
            {
                if (parent.Datum.type == Datum.DatumType.R_OBJECT)
                {
                    parent.Datum.r_object.Add(new Datum.AssocPair()
                        {
                            key = propertyName,
                            val = writerToken.Datum
                        });
                    propertyName = null;
                }
                else
                {
                    parent.Datum.r_array.Add(writerToken.Datum);
                }
                writerToken.Parent = parent;
            }
            else
            {
                if (writerToken.Datum.type != Datum.DatumType.R_OBJECT && writerToken.Datum.type != Datum.DatumType.R_ARRAY)
                    throw new JsonException(
                        string.Format("Error writing {0} value. Datum must start with an Object or Array.", writerToken.Datum.type), null);

                parent = writerToken;
                root = writerToken;
            }
        }

        public override void WriteStartObject()
        {
            base.WriteStartObject();
            AddParent(new DatumWriterToken(Datum.DatumType.R_OBJECT));
        }

        public override void WriteStartArray()
        {
            base.WriteStartArray();
            AddParent(new DatumWriterToken(Datum.DatumType.R_ARRAY));
        }

        public override void WritePropertyName(string name)
        {
            base.WritePropertyName(name);
            propertyName = name;
        }

        protected override void WriteEnd(JsonToken token)
        {
            base.WriteEnd(token);
            RemoveParent();
        }

        public override void WriteNull()
        {
            base.WriteNull();
            AddValue(new Datum() {type = Datum.DatumType.R_NULL});
        }

        public override void WriteUndefined()
        {
            base.WriteUndefined();
            AddValue(new Datum() {type = Datum.DatumType.R_NULL});
        }

        protected virtual void WritePrimitive<T>(T obj)
        {
            var datum = PrimitiveDatumConverterFactory.Instance.Get<T>()
                .ConvertObject(obj);

            AddValue(datum);
        }

        protected virtual void WriteNullable<T>(T obj)
        {
            var datum = NullableDatumConverterFactory.Instance.Get<T>()
                .ConvertObject(obj);

            AddValue(datum);
        }

        public override void WriteValue(string value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(int value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(uint value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(long value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(ulong value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(float value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(double value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(bool value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(short value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(ushort value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(char value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(byte value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);
            WritePrimitive(value);
        }

        public override void WriteValue(Guid value)
        {
            base.WriteValue(value);

            var datum = GuidDatumConverter.Instance.Value.ConvertObject(value);

            AddValue(datum);
        }

        public override void WriteValue(byte[] value)
        {
            base.WriteValue(value);

            var d = ArrayDatumConverterFactory.Instance.Get<byte[]>(
                PrimitiveDatumConverterFactory.Instance
                ).ConvertObject(value);
            AddValue(d);
        }

        public override void WriteValue(Uri value)
        {
            base.WriteValue(value);

            var d = UriDatumConverterFactory.Instance.Get<Uri>().ConvertObject(value);
            AddValue(d);
        }

        public override void WriteValue(DateTime value)
        {
            base.WriteValue(value);

            var d = DateTimeDatumConverter.Instance.Value.ConvertObject(value);

            AddValue(d);
        }

        public override void WriteValue(DateTimeOffset value)
        {
            base.WriteValue(value);

            var d = DateTimeOffsetDatumConverter.Instance.Value.ConvertObject(value);

            AddValue(d);
        }

        public override void WriteValue( TimeSpan value )
        {
            //base.WriteValue(value);
            //WritePrimitive(value.TotalSeconds);
            throw new JsonException( "TimeSpans can only be converted by including the DatumTimeSpanConverter in JsonSeralizerSettings. See source comment for: DatumTimeSpanConverter." );
        }

        public override void WriteValue( TimeSpan? value )
        {
            //base.WriteValue(value);
            //WriteNullable(value == null ? (double?)null : value.Value.TotalSeconds);
            throw new JsonException( "TimeSpans can only be converted by including the DatumTimeSpanConverter in JsonSeralizerSettings. See source comment for: DatumTimeSpanConverter." );

        }

        #region Not Implemented
        public override void WriteRaw(string json)
        {
            throw new JsonException("Not Valid Operation for Datum.", null);
        }

        public override void WriteRawValue(string json)
        {
            throw new JsonException("Not Valid Operation for Datum.", null);
        }

        public override void WriteComment(string text)
        {
            throw new JsonException("Not Valid Operation for Datum.", null);
        }

        public override void WriteStartConstructor(string name)
        {
            throw new JsonException("Not Valid Operation for Datum.", null);
        }
        #endregion
    }
}