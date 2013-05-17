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

        public bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(DateTime))
                datumConverter = (IDatumConverter<T>)DateTimeDatumConverter.Instance.Value;

            return datumConverter != null;
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
}