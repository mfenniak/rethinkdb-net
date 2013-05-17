using System;

namespace RethinkDb
{
    public static class DatumConverterFactoryExtensions
    {
        public static bool TryGet<T>(this IDatumConverterFactory datumConverterFactory, out IDatumConverter<T> datumConverter)
        {
            return datumConverterFactory.TryGet<T>(datumConverterFactory, out datumConverter);
        }

        public static IDatumConverter<T> Get<T>(this IDatumConverterFactory datumConverterFactory)
        {
            if (datumConverterFactory == null)
                throw new ArgumentNullException("datumConverterFactory");
            return datumConverterFactory.Get<T>(datumConverterFactory);
        }

        public static IDatumConverter<T> Get<T>(this IDatumConverterFactory datumConverterFactory, IDatumConverterFactory rootDatumConverterFactory)
        {
            if (datumConverterFactory == null)
                throw new ArgumentNullException("datumConverterFactory");
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");
            IDatumConverter<T> retval;
            if (datumConverterFactory.TryGet<T>(rootDatumConverterFactory, out retval))
                return retval;
            else
                throw new NotSupportedException(String.Format("Datum converter is not availble for type {0}", typeof(T)));
        }
    }
}

