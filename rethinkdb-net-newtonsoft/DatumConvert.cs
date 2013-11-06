using System;
using Newtonsoft.Json;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft
{
    public static class DatumConvert
    {
        public static Datum SerializeObject(object value)
        {
            return SerializeObject(value, (JsonSerializerSettings)null);
        }

        public static Datum SerializeObject(object value, params JsonConverter[] converters)
        {
            JsonSerializerSettings settings = (converters != null && converters.Length > 0)
                ? new JsonSerializerSettings {Converters = converters}
                : null;

            return SerializeObject(value, settings);
        }

        public static Datum SerializeObject(object value, JsonSerializerSettings settings)
        {
            return SerializeObject(value, null, settings);
        }

        public static Datum SerializeObject(object value, Type type, JsonSerializerSettings settings)
        {
            var serializer = JsonSerializer.CreateDefault(settings);

            Datum datum;
            using (var datumWriter = new DatumWriter())
            {
                serializer.Serialize(datumWriter, value, type);

                datum = datumWriter.GetRootDatum();
            }

            return datum;
        }

        public static object DeserializeObject(Datum value)
        {
            return DeserializeObject(value, null, (JsonSerializerSettings)null);
        }

        public static object DeserializeObject(Datum value, JsonSerializerSettings settings)
        {
            return DeserializeObject(value, null, settings);
        }

        public static object DeserializeObject(Datum value, Type type)
        {
            return DeserializeObject(value, type, (JsonSerializerSettings)null);
        }

        public static T DeserializeObject<T>(Datum value)
        {
            return DeserializeObject<T>(value, (JsonSerializerSettings)null);
        }

        public static T DeserializeAnonymousType<T>(Datum value, T anonymousTypeObject)
        {
            return DeserializeObject<T>(value);
        }

        public static T DeserializeAnonymousType<T>(Datum value, T anonymousTypeObject, JsonSerializerSettings settings)
        {
            return DeserializeObject<T>(value, settings);
        }

        public static T DeserializeObject<T>(Datum value, params JsonConverter[] converters)
        {
            return (T)DeserializeObject(value, typeof(T), converters);
        }

        public static T DeserializeObject<T>(Datum value, JsonSerializerSettings settings)
        {
            return (T)DeserializeObject(value, typeof(T), settings);
        }

        public static object DeserializeObject(Datum value, Type type, params JsonConverter[] converters)
        {
            JsonSerializerSettings settings = (converters != null && converters.Length > 0)
                ? new JsonSerializerSettings {Converters = converters}
                : null;

            return DeserializeObject(value, type, settings);
        }

        public static object DeserializeObject(Datum value, Type type, JsonSerializerSettings settings)
        {
            Demand.Require(value != null, "Can't deserialize null value.");

            var serializer = JsonSerializer.CreateDefault(settings);

            return serializer.Deserialize(new DatumReader(value), type);
        }
    }
}