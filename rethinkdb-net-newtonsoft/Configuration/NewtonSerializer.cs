using RethinkDb.DatumConverters;

namespace RethinkDb.Newtonsoft.Configuration
{
    public class NewtonSerializer : AggregateDatumConverterFactory
    {
        public NewtonSerializer() : base(
            PrimitiveDatumConverterFactory.Instance,

            AnonymousTypeDatumConverterFactory.Instance,

            NewtonsoftDatumConverterFactory.Instance
            )
        {
        }
    }
}