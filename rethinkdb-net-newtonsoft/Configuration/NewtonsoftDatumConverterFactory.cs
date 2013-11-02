using RethinkDb.DatumConverters;

namespace RethinkDb.Newtonsoft.Configuration
{
    public class NewtonsoftDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly NewtonsoftDatumConverterFactory Instance = new NewtonsoftDatumConverterFactory();

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            //I guess we could have some more specific checks here
            //but if we get here last in the NewtonsoftSerializer order, then
            //I suppose we can handle it if no preceding converters could handle it. 
            datumConverter = new NewtonsoftDatumConverter<T>();
            return true;
        }
    }
}