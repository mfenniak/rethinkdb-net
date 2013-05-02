using System;
using RethinkDb.Spec;

namespace RethinkDb
{
    public class DateTimeOffsetDatumConverterFactory : IDatumConverterFactory
    {
        public static readonly DateTimeOffsetDatumConverterFactory Instance = new DateTimeOffsetDatumConverterFactory();

        public DateTimeOffsetDatumConverterFactory()
        {
        }

        public IDatumConverter<T> Get<T>()
        {
            if (typeof(T) == typeof(DateTimeOffset))
                return (IDatumConverter<T>)DateTimeOffsetDatumConverter.Instance.Value;
            else if(typeof(T) == typeof(DateTimeOffset?))
                return (IDatumConverter<T>)NullableDateTimeOffsetDatumConverter.Instance.Value;
            else
                throw new NotSupportedException(String.Format("Type {0} is not supported by DateTimeOffsetDatumConverterFactory", typeof(T)));        
        }
    }

    public class DateTimeOffsetDatumConverter : IDatumConverter<DateTimeOffset>
    {
        public static readonly Lazy<DateTimeOffsetDatumConverter> Instance = new Lazy<DateTimeOffsetDatumConverter>(() => new DateTimeOffsetDatumConverter());

        public static string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fffK";

        public DateTimeOffset ConvertDatum(Datum datum)
        {
            DateTimeOffset dateTimeOffset;
            if (!DateTimeOffset.TryParse(datum.r_str, out dateTimeOffset))
                throw new Exception(string.Format("Not valid serialized DateTimeOffset: {0}", datum.r_str));

            return dateTimeOffset;
        }

        public Datum ConvertObject(DateTimeOffset dateTimeOffset)
        {
            return new Datum() { type = Datum.DatumType.R_STR, r_str = dateTimeOffset.ToUniversalTime().ToString(DateTimeOffsetDatumConverter.DateTimeFormat) };
        }
    }

    public class NullableDateTimeOffsetDatumConverter : IDatumConverter<DateTimeOffset?>
    {
        public static readonly Lazy<NullableDateTimeOffsetDatumConverter> Instance = new Lazy<NullableDateTimeOffsetDatumConverter>(() => new NullableDateTimeOffsetDatumConverter());

        public static string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fffK";

        public DateTimeOffset? ConvertDatum(Datum datum)
        {
            if (datum.type == Datum.DatumType.R_NULL)
                return null;
            else
            {
                DateTimeOffset dateTimeOffset;
                if (!DateTimeOffset.TryParse(datum.r_str, out dateTimeOffset))
                    throw new Exception(string.Format("Not valid serialized DateTimeOffset: {0}", datum.r_str));

                return dateTimeOffset;
            }                
        }

        public Datum ConvertObject(DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset.HasValue)
                return new Datum() { type = Datum.DatumType.R_STR, r_str = dateTimeOffset.Value.ToUniversalTime().ToString(NullableDateTimeOffsetDatumConverter.DateTimeFormat) };
            else
                return new Datum() { type = Datum.DatumType.R_NULL };
        }
    }
}

