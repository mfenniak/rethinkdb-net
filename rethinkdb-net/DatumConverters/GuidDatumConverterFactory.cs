using System;

namespace RethinkDb
{
    public class GuidDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly GuidDatumConverterFactory Instance = new GuidDatumConverterFactory();

        public GuidDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(Guid))
                datumConverter = (IDatumConverter<T>)GuidDatumConverter.Instance.Value;

            return datumConverter != null;
        }
    }

    public class GuidDatumConverter : AbstractValueTypeDatumConverter<Guid>
    {
        public static readonly Lazy<GuidDatumConverter> Instance = new Lazy<GuidDatumConverter>(() => new GuidDatumConverter());

        #region IDatumConverter<Guid> Members

        public override Guid ConvertDatum(Spec.Datum datum)
        {
            Guid guid;
            if (Guid.TryParse(datum.r_str, out guid))
                return guid;            
            else            
                throw new Exception(string.Format("Not valid serialized Guid: {0}", datum.r_str));
        }

        public override Spec.Datum ConvertObject(Guid guid)
        {                
            return new Spec.Datum() { type = Spec.Datum.DatumType.R_STR, r_str = guid.ToString() };
        }

        #endregion
    }
}
