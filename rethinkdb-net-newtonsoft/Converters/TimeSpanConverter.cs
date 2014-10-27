using System;
using Newtonsoft.Json;

namespace RethinkDb.Newtonsoft.Converters
{
    /// <summary>
    /// Important note about converters:
    /// 
    /// Newtonsoft will call Read() on your behalf to advance
    /// the reader and set the reader.Value.
    /// 
    /// Regardless of the type, whenever a converter is used, Read() on the
    /// DatumWriter is always called even if Newtonsoft encounters a type
    /// it knows about (like a DateTime or Decimal).
    /// </summary>
    public class TimeSpanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var span = (TimeSpan)value;

            writer.WriteValue(span.TotalSeconds);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var floatSeconds = reader.Value as double?;

            if (floatSeconds.HasValue)
            {
                return TimeSpan.FromSeconds(floatSeconds.Value);
            }

            if (objectType == typeof(TimeSpan?))
                return default(TimeSpan?);

            return default(TimeSpan);
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?))
                return true;

            return false;
        }
    }
}