using RethinkDb.DatumConverters;

namespace RethinkDb.Newtonsoft.Test
{
    public class Native
    {
        public static AggregateDatumConverterFactory RootFactory = new AggregateDatumConverterFactory(
            PrimitiveDatumConverterFactory.Instance,
            DataContractDatumConverterFactory.Instance,
            DateTimeDatumConverterFactory.Instance,
            DateTimeOffsetDatumConverterFactory.Instance,
            GuidDatumConverterFactory.Instance,
            UriDatumConverterFactory.Instance,
            TupleDatumConverterFactory.Instance,
            ArrayDatumConverterFactory.Instance,
            AnonymousTypeDatumConverterFactory.Instance,
            EnumDatumConverterFactory.Instance,
            NullableDatumConverterFactory.Instance,
            ListDatumConverterFactory.Instance
            );
    }
}