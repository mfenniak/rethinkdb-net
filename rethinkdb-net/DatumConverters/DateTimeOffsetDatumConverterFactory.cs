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

        public bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(DateTimeOffset))
                datumConverter = (IDatumConverter<T>)DateTimeOffsetDatumConverter.Instance.Value;

            return datumConverter != null;
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
}

