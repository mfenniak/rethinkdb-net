using System;
using System.Linq;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public class BinaryDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly BinaryDatumConverterFactory Instance = new BinaryDatumConverterFactory();

        public BinaryDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(byte[]))
                datumConverter = (IDatumConverter<T>)BinaryDatumConverter.Instance.Value;

            return datumConverter != null;
        }
    }

    public class BinaryDatumConverter : AbstractReferenceTypeDatumConverter<byte[]>
    {
        public static readonly Lazy<BinaryDatumConverter> Instance = new Lazy<BinaryDatumConverter>(() => new BinaryDatumConverter());

        #region IDatumConverter<Guid> Members

        public override byte[] ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Datum.DatumType.R_NULL)
                return null;
            else if (datum.type != Datum.DatumType.R_OBJECT)
                throw new NotSupportedException("Attempted to cast Datum to byte[], but Datum was unexpected type " + datum.type);

            var keys = datum.r_object.ToDictionary(kvp => kvp.key, kvp => kvp.val);

            Datum reql_type;
            if (!keys.TryGetValue("$reql_type$", out reql_type) || reql_type.type != Datum.DatumType.R_STR || reql_type.r_str != "BINARY")
                throw new NotSupportedException("Attempted to cast OBJECT to byte[], but Datum was not $reql_type$ = BINARY");

            Datum data;
            if (!keys.TryGetValue("data", out data) || data.type != Datum.DatumType.R_STR)
                throw new NotSupportedException("Attempted to cast OBJECT to byte[], but object was missing data field");

            return Convert.FromBase64String(data.r_str);
        }

        public override Datum ConvertObject(byte[] bytes)
        {
            if (bytes == null)
                return new Datum() { type = Spec.Datum.DatumType.R_NULL };

            var datum = new Datum() {
                type = Datum.DatumType.R_OBJECT
            };
            datum.r_object.Add(new Datum.AssocPair() {
                key = "$reql_type$",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "BINARY"
                }
            });
            datum.r_object.Add(new Datum.AssocPair() {
                key = "data",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = Convert.ToBase64String(bytes),
                }
            });
            return datum;
        }

        #endregion
    }
}
