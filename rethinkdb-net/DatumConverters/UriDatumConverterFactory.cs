using System;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public class UriDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly UriDatumConverterFactory Instance = new UriDatumConverterFactory();

        public UriDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(Uri))
                datumConverter = (IDatumConverter<T>)UriDatumConverter.Instance.Value;

            return datumConverter != null;
        }
    }

    public class UriDatumConverter : AbstractReferenceTypeDatumConverter<Uri>
    {
        public static readonly Lazy<UriDatumConverter> Instance = new Lazy<UriDatumConverter>(() => new UriDatumConverter());

        #region IDatumConverter<Uri> Members

        public override Uri ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Datum.DatumType.R_NULL)
                return null;

            Uri uri;
            if (Uri.TryCreate(datum.r_str, UriKind.Absolute, out uri))
                return uri;            
            else            
                throw new Exception(string.Format("Not valid serialized Uri: {0}", datum.r_str));
        }

        public override Spec.Datum ConvertObject(Uri uri)
        {                
            if (uri == null)
                return new Datum() { type = Datum.DatumType.R_NULL };

            return new Spec.Datum() {
                type = Spec.Datum.DatumType.R_STR,
                r_str = uri.OriginalString
            };
        }

        #endregion
    }
}
