using System;
using System.Linq;
using System.Text;
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

        public DateTimeOffset ConvertDatum(Datum datum)
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

            TimeSpan offset;
            char sign;
            string h, m;
            if (timezone.r_str == "Z")
            {
                sign = '+';
                h = "00";
                m = "00";
            }
            else if (timezone.r_str.Length == 6)
            {
                // [+-]\d\d:\d\d
                sign = timezone.r_str[0];
                h = timezone.r_str.Substring(1, 2);
                m = timezone.r_str.Substring(4, 2);
            }
            else if (timezone.r_str.Length == 5)
            {
                // [+-]\d\d\d\d
                sign = timezone.r_str[0];
                h = timezone.r_str.Substring(1, 2);
                m = timezone.r_str.Substring(3, 2);
            }
            else if (timezone.r_str.Length == 3)
            {
                // [+-]\d\d
                sign = timezone.r_str[0];
                h = timezone.r_str.Substring(1, 2);
                m = "00";
            }
            else
                throw new FormatException(String.Format("Unexpected timezone format: {0}; unexpected length", timezone.r_str));

            int hours, minutes;
            if (!int.TryParse(h, out hours))
                throw new FormatException(String.Format("Unexpected timezone format: {0}; hours couldn't be parsed", timezone.r_str));
            if (!int.TryParse(m, out minutes))
                throw new FormatException(String.Format("Unexpected timezone format: {0}; minutes couldn't be parsed", timezone.r_str));
            offset = new TimeSpan(hours, minutes, 0);
            if (sign == '-')
                offset = -offset;
            else if (sign != '+')
                throw new FormatException(String.Format("Unexpected timezone format: {0}; sign couldn't be parsed", timezone.r_str));

            return new DateTimeOffset((long)(epoch_time.r_num * 10000000) + 621355968000000000, offset);
        }

        public Datum ConvertObject(DateTimeOffset dateTimeOffset)
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
                    r_num = (dateTimeOffset.DateTime.Ticks - 621355968000000000) / 10000000.0
                }
            });

            StringBuilder offset = new StringBuilder(5);
            if (dateTimeOffset.Offset < TimeSpan.Zero)
                offset.Append('-');
            else
                offset.Append('+');
            offset.AppendFormat("{0:00}", Math.Abs(dateTimeOffset.Offset.Hours));
            offset.Append(':');
            offset.AppendFormat("{0:00}", Math.Abs(dateTimeOffset.Offset.Minutes));

            datum.r_object.Add(new Datum.AssocPair() {
                key = "timezone",
                val = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = offset.ToString(),
                }
            });
            return datum;
        }
    }
}

