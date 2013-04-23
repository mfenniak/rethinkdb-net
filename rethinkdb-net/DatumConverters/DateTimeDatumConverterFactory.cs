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
            else
                throw new NotSupportedException(String.Format("Type {0} is not supported by DateTimeDatumConverterFactory", typeof(T)));        
        }
    }

    public class DateTimeDatumConverter : IDatumConverter<DateTime>
    {
        public static readonly Lazy<DateTimeDatumConverter> Instance = new Lazy<DateTimeDatumConverter>(() => new DateTimeDatumConverter());

        public DateTime ConvertDatum(Datum datum)
        {
            return new DateTime(long.Parse(datum.r_str.Replace("/Date(", "").Replace(")/","")));
        }

        public Datum ConvertObject(DateTime dateTime)
        {
            return new Datum() { type = Datum.DatumType.R_STR, r_str = string.Format(@"/Date({0})/", dateTime.Ticks) };
        }
    }
}