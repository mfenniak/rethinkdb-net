using System;
using RethinkDb.Spec;

namespace RethinkDb.DatumConverters
{
    public class TimeSpanDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly TimeSpanDatumConverterFactory Instance = new TimeSpanDatumConverterFactory();

        private TimeSpanDatumConverterFactory()
        {
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            datumConverter = null;

            if (typeof(T) == typeof(TimeSpan))
                datumConverter = (IDatumConverter<T>)TimeSpanDatumConverter.Instance.Value;

            return datumConverter != null;
        }
    }

    public class TimeSpanDatumConverter : AbstractValueTypeDatumConverter<TimeSpan>
    {
        public static readonly Lazy<TimeSpanDatumConverter> Instance = new Lazy<TimeSpanDatumConverter>(() => new TimeSpanDatumConverter());

        public override TimeSpan ConvertDatum(Datum datum)
        {
            if (datum.type != Datum.DatumType.R_NUM)
                throw new NotSupportedException("Attempted to cast Datum to TimeSpan, but Datum was unexpected type " + datum.type + "; expected R_NUM");
            return TimeSpan.FromSeconds(datum.r_num);
        }

        public override Datum ConvertObject(TimeSpan timeSpan)
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_NUM,
                r_num = timeSpan.TotalSeconds,
            };
            return datum;
        }
    }
}