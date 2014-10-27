using System;
using RethinkDb.DatumConverters;

namespace RethinkDb.Newtonsoft.Configuration
{
    public class NewtonsoftDatumConverterFactory : AbstractDatumConverterFactory
    {
        public static readonly NewtonsoftDatumConverterFactory Instance = new NewtonsoftDatumConverterFactory();

        public override bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            // Use rethinkdb-net's support for $reql_type$=GROUPED_DATA return values.
            if (GroupingDictionaryDatumConverterFactory.Instance.TryGet<T>(rootDatumConverterFactory, out datumConverter))
                return true;

            //I guess we could have some more specific checks here
            //but if we get here last in the NewtonsoftSerializer order, then
            //I suppose we can handle it if no preceding converters could handle it. 


            //makes no difference to Newtonsoft, but base class constructor 
            //checks this constraint and throws if it's not exactly a Value type converter...
            if (typeof(T).IsValueType)
            {
                datumConverter = new NewtonsoftValueDatumConverter<T>();
                return true;
            }

            datumConverter = new NewtonsoftReferenceDatumConverter<T>();
            return true;
        }
    }
}