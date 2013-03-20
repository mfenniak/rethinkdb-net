using System;
using System.IO;
using System.Text;
using RethinkDb.Spec;

namespace RethinkDb
{
    class DataContractDatumConverter<T> : IDatumConverter<T>
    {
        private System.Runtime.Serialization.Json.DataContractJsonSerializer dcs;

        public DataContractDatumConverter()
        {
            dcs = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
        }

        public DataContractDatumConverter(System.Runtime.Serialization.Json.DataContractJsonSerializer dcs)
        {
            this.dcs = dcs;
        }

        public T ConvertDatum(Datum datum)
        {
            // FIXME: the DataContractJsonSerializer approach seems completely wrong with the new rethinkdb protocol;
            // will need to figure out now how to map Datum objects into native objects ourselves.  In the mean time,
            // actually convert the Datum to Json text so that we can use DataContractJsonSerializer.

            StringBuilder builder = new StringBuilder();
            ConvertDatumToJson(builder, datum);

            var data = Encoding.UTF8.GetBytes(builder.ToString());
            using (var stream = new MemoryStream(data))
            {
                return (T)dcs.ReadObject(stream);
            }
        }

        public Datum ConvertObject(T obj)
        {
            var retval = new Datum()
            {
                type = Datum.DatumType.R_OBJECT,
            };

            retval.r_object.Add(new Datum.AssocPair() {
                key = "name",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "Test #55",
                }
            });

            return retval;
        }

        private void ConvertDatumToJson(StringBuilder builder, Datum datum)
        {
            switch (datum.type)
            {
                case Datum.DatumType.R_ARRAY:
                    builder.Append('[');
                    for (int i = 0; i < datum.r_array.Count; i++)
                    {
                        ConvertDatumToJson(builder, datum.r_array[i]);
                        if (i != (datum.r_array.Count - 1))
                            builder.Append(',');
                    }
                    builder.Append(']');
                    break;
                case Datum.DatumType.R_BOOL:
                    if (datum.r_bool)
                        builder.Append("true");
                    else
                        builder.Append("false");
                    break;
                case Datum.DatumType.R_NULL:
                    builder.Append("null");
                    break;
                case Datum.DatumType.R_NUM:
                    builder.Append(datum.r_num);
                    break;
                case Datum.DatumType.R_OBJECT:
                    builder.Append('{');
                    for (int i = 0; i < datum.r_object.Count; i++)
                    {
                        var key = datum.r_object[i].key;
                        var value = datum.r_object[i].val;
                        ConvertStringToJson(builder, key);
                        builder.Append(':');
                        ConvertDatumToJson(builder, value);
                        if (i != (datum.r_object.Count - 1))
                            builder.Append(',');
                    }
                    builder.Append('}');
                    break;
                case Datum.DatumType.R_STR:
                    ConvertStringToJson(builder, datum.r_str);
                    break;
                default:
                    throw new NotSupportedException("Unsupported datum type");
            }
        }

        private void ConvertStringToJson(StringBuilder builder, string str)
        {
            builder.Append('"');
            foreach (var c in str)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }
            builder.Append('"');
        }
    }
}
