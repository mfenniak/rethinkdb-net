using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Newtonsoft.Configuration
{
    public class NewtonsoftReferenceDatumConverter<T> : AbstractReferenceTypeDatumConverter<T>, IObjectDatumConverter
    {
        public override T ConvertDatum(Datum datum)
        {
            return DatumConvert.DeserializeObject<T>(datum, ConfigurationAssembler.DefaultJsonSerializerSettings);
        }

        public override Datum ConvertObject(T value)
        {
            return DatumConvert.SerializeObject(value, ConfigurationAssembler.DefaultJsonSerializerSettings);
        }

        public string GetDatumFieldName(MemberInfo memberInfo)
        {
            var contract = ConfigurationAssembler
                .DefaultJsonSerializerSettings
                .ContractResolver
                .ResolveContract(typeof(T))
                as JsonObjectContract;

            return contract.Properties
                .First(p => p.UnderlyingName == memberInfo.Name)
                .PropertyName;
        }
    }

    public class NewtonsoftValueDatumConverter<T> : AbstractValueTypeDatumConverter<T>
    {
        public override T ConvertDatum(Datum datum)
        {
            return DatumConvert.DeserializeObject<T>(datum, ConfigurationAssembler.DefaultJsonSerializerSettings);
        }

        public override Datum ConvertObject(T value)
        {
            return DatumConvert.SerializeObject(value, ConfigurationAssembler.DefaultJsonSerializerSettings);
        }
    }
}