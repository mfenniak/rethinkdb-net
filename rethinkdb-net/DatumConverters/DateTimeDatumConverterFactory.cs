using System;
using System.Linq;
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

        public DateTime ConvertDatum(Datum datum)
        {
            if (datum.type != Datum.DatumType.R_OBJECT)
                throw new NotSupportedException("Attempted to cast Datum to DateTime, but Datum was unsupported type " + datum.type);

            var keys = datum.r_object.ToDictionary(kvp => kvp.key, kvp => kvp.val);

            Datum reql_type;
            if (!keys.TryGetValue("$reql_type$", out reql_type) || reql_type.type != Datum.DatumType.R_STR || reql_type.r_str != "TIME")
                throw new NotSupportedException("Attempted to cast OBJECT to DateTime, but Datum was not $reql_type$ = TIME");

            Datum epoch_time;
            if (!keys.TryGetValue("epoch_time", out epoch_time) || epoch_time.type != Datum.DatumType.R_NUM)
                throw new NotSupportedException("Attempted to cast OBJECT to DateTime, but object was missing epoch_time field");

            Datum timezone;
            if (!keys.TryGetValue("timezone", out timezone) || timezone.type != Datum.DatumType.R_STR)
                throw new NotSupportedException("Attempted to cast OBJECT to DateTime, but object was missing timezone field");
            else if (timezone.r_str != "+00:00" && timezone.r_str != "Z")
                throw new NotSupportedException("UTC time zone supported only");

            return new DateTime((long)(epoch_time.r_num * 10000000) + 621355968000000000, DateTimeKind.Utc);
        }

        public Datum ConvertObject(DateTime dateTime)
        {
            var datum = new Datum() {
                type = Datum.DatumType.R_OBJECT
            };
            datum.r_object.Add(new Datum.AssocPair() {
                key = "$reql_type$",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "TIME"
                }
            });
            datum.r_object.Add(new Datum.AssocPair() {
                key = "epoch_time",
                val = new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = (dateTime.Ticks - 621355968000000000) / 10000000.0
                }
            });
            datum.r_object.Add(new Datum.AssocPair() {
                key = "timezone",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = "+00:00"
                }
            });
            return datum;
        }
    }
}