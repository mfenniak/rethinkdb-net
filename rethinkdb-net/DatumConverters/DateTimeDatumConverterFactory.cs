using System;
using RethinkDb.Spec;

namespace RethinkDb
{
    public class DateTimeDatumConverterFactory : IDatumConverterFactory
    {
        public static readonly DateTimeDatumConverterFactory Instance = new DateTimeDatumConverterFactory();

        public DateTimeDatumConverterFactory()
        {
        }

        public IDatumConverter<T> Get<T>()
        {
            if (typeof(T) == typeof(DateTime))
                return (IDatumConverter<T>)DateTimeDatumConverter.Instance.Value;
            else if(typeof(T) == typeof(DateTime?))
                return (IDatumConverter<T>)NullableDateTimeDatumConverter.Instance.Value;
            else
                throw new NotSupportedException(String.Format("Type {0} is not supported by DateTimeDatumConverterFactory", typeof(T)));        
        }
    }

    public class DateTimeDatumConverter : IDatumConverter<DateTime>
    {
        public static readonly Lazy<DateTimeDatumConverter> Instance = new Lazy<DateTimeDatumConverter>(() => new DateTimeDatumConverter());

        public static string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fffK";

        public DateTime ConvertDatum(Datum datum)
        {
            DateTime dateTime;
            if (!DateTime.TryParse(datum.r_str, out dateTime))
                throw new Exception(string.Format("Not valid serialized DateTime: {0}", datum.r_str));

            return dateTime;
        }

        public Datum ConvertObject(DateTime dateTime)
        {
            return new Datum() { type = Datum.DatumType.R_STR, r_str = dateTime.ToUniversalTime().ToString(DateTimeDatumConverter.DateTimeFormat) };
        }
    }

    public class NullableDateTimeDatumConverter : IDatumConverter<DateTime?>
    {
        public static readonly Lazy<NullableDateTimeDatumConverter> Instance = new Lazy<NullableDateTimeDatumConverter>(() => new NullableDateTimeDatumConverter());

        public static string DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fffK";

        public DateTime? ConvertDatum(Datum datum)
        {
            if (datum.type == Datum.DatumType.R_NULL)
                return null;
            else
            {
                DateTime dateTime;
                if (!DateTime.TryParse(datum.r_str, out dateTime))
                    throw new Exception(string.Format("Not valid serialized DateTime: {0}", datum.r_str));

                return dateTime;
            }                
        }

        public Datum ConvertObject(DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return new Datum() { type = Datum.DatumType.R_STR, r_str = dateTime.Value.ToUniversalTime().ToString(DateTimeDatumConverter.DateTimeFormat) };
            else
                return new Datum() { type = Datum.DatumType.R_NULL };
        }
    }
}