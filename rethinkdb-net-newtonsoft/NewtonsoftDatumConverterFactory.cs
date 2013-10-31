using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RethinkDb.DatumConverters;
using RethinkDb.Newtonsoft.Converters;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft
{
    public class NewtonSerializer : AggregateDatumConverterFactory
    {
        public NewtonSerializer() : base(
            PrimitiveDatumConverterFactory.Instance,

            TupleDatumConverterFactory.Instance,

            AnonymousTypeDatumConverterFactory.Instance,

            NewtonsoftDatumConverterFactory.Instance
            )
        {
        }
    }

    public class NewtonsoftDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly NewtonsoftDatumConverterFactory Instance = new NewtonsoftDatumConverterFactory();

        public static JsonSerializerSettings DefaultSeralizerSettings { get; set; }

        public NewtonsoftDatumConverterFactory()
        {
            DefaultSeralizerSettings = new JsonSerializerSettings
                {
                    Converters =
                        {
                            new TimeSpanConverter()
                        },
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
        }

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            //I guess we could have some more specific checks here
            //but if we get here last in the NewtonsoftSerializer order, then
            //I suppose we can handle it if no preceding converters could handle it. 
            datumConverter = new NewtonsoftDatumConverter<T>(DefaultSeralizerSettings);
            return true;
        }
    }

    public class NewtonsoftDatumConverter<T> : AbstractReferenceTypeDatumConverter<T>
    {
        private readonly JsonSerializerSettings settings;

        public NewtonsoftDatumConverter(JsonSerializerSettings settings)
        {
            this.settings = settings;
        }

        public override T ConvertDatum(Datum datum)
        {
            return DatumConvert.DeserializeObject<T>( datum, settings );
        }

        public override Datum ConvertObject(T value)
        {
            return DatumConvert.SerializeObject(value, settings);
        }
    }

}