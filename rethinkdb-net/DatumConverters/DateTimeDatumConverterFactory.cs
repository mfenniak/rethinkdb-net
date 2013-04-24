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

        public DateTime ConvertDatum(Datum datum)
        {
            var ticksString = datum.r_str.Replace("/Date(", "").Replace(")/","");

            long ticks;
            if (!Int64.TryParse(ticksString, out ticks))
                throw new Exception(string.Format("Not valid serialized DateTime: {0}", datum.r_str));

            return new DateTime(ticks);
        }

        public Datum ConvertObject(DateTime dateTime)
        {
            return new Datum() { type = Datum.DatumType.R_STR, r_str = string.Format(@"/Date({0})/", dateTime.Ticks) };
        }
    }

    public class NullableDateTimeDatumConverter : IDatumConverter<DateTime?>
    {
        public static readonly Lazy<NullableDateTimeDatumConverter> Instance = new Lazy<NullableDateTimeDatumConverter>(() => new NullableDateTimeDatumConverter());

        public DateTime? ConvertDatum(Datum datum)
        {
            if (datum.type == Datum.DatumType.R_NULL)
                return null;
            else
            {
                var ticksString = datum.r_str.Replace("/Date(", "").Replace(")/","");

                long ticks;
                if (!Int64.TryParse(ticksString, out ticks))
                    throw new Exception(string.Format("Not valid serialized DateTime: {0}", datum.r_str));

                return new DateTime(ticks);
            }                
        }

        public Datum ConvertObject(DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return new Datum() { type = Datum.DatumType.R_STR, r_str = string.Format(@"/Date({0})/", dateTime.Value.Ticks) };
            else
                return new Datum() { type = Datum.DatumType.R_NULL };
        }
    }
}