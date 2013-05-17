using System;

namespace RethinkDb
{
    public class GuidDatumConverterFactory : IDatumConverterFactory
    {
        public static readonly GuidDatumConverterFactory Instance = new GuidDatumConverterFactory();

        public GuidDatumConverterFactory()
        {
        }

        public bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(Guid))
                datumConverter = (IDatumConverter<T>)GuidDatumConverter.Instance.Value;
            else if(typeof(T) == typeof(Guid?))
                datumConverter = (IDatumConverter<T>)NullableGuidDatumConverter.Instance.Value;

            return datumConverter != null;
        }
    }

    public class GuidDatumConverter : IDatumConverter<Guid>
    {
        public static readonly Lazy<GuidDatumConverter> Instance = new Lazy<GuidDatumConverter>(() => new GuidDatumConverter());

        #region IDatumConverter<Guid> Members

        public Guid ConvertDatum(Spec.Datum datum)
        {
            Guid guid;
            if (Guid.TryParse(datum.r_str, out guid))
                return guid;            
            else            
                throw new Exception(string.Format("Not valid serialized Guid: {0}", datum.r_str));
        }

        public Spec.Datum ConvertObject(Guid guid)
        {                
            return new Spec.Datum() { type = Spec.Datum.DatumType.R_STR, r_str = guid.ToString() };
        }

        #endregion
    }

    public class NullableGuidDatumConverter : IDatumConverter<Guid?>
    {
        public static readonly Lazy<NullableGuidDatumConverter> Instance = new Lazy<NullableGuidDatumConverter>(() => new NullableGuidDatumConverter());

        public Guid? ConvertDatum(Spec.Datum datum)
        {
            if (datum.type == Spec.Datum.DatumType.R_NULL)
                return null;
            else
            {
                Guid guid;
                if (Guid.TryParse(datum.r_str, out guid))
                    return guid;            
                else            
                    throw new Exception(string.Format("Not valid serialized Guid: {0}", datum.r_str));
            }                
        }

        public Spec.Datum ConvertObject(Guid? guid)
        {
            if (guid.HasValue)
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_STR, r_str = guid.Value.ToString() };
            else
                return new Spec.Datum() { type = Spec.Datum.DatumType.R_NULL };
        }
    }
}

