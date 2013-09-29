using System;

namespace RethinkDb.DatumConverters
{
    public static class DatumConverterFactoryExtensions
    {
        #region Generic extensions

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
                throw new NotSupportedException(String.Format("Datum converter is not available for type {0}", typeof(T)));
        }

        #endregion
        #region Non-generic extensions

        public static bool TryGet(this IDatumConverterFactory datumConverterFactory, Type datumType, out IDatumConverter datumConverter)
        {
            return datumConverterFactory.TryGet(datumType, datumConverterFactory, out datumConverter);
        }

        public static IDatumConverter Get(this IDatumConverterFactory datumConverterFactory, Type datumType)
        {
            if (datumConverterFactory == null)
                throw new ArgumentNullException("datumConverterFactory");
            return datumConverterFactory.Get(datumType, datumConverterFactory);
        }

        public static IDatumConverter Get(this IDatumConverterFactory datumConverterFactory, Type datumType, IDatumConverterFactory rootDatumConverterFactory)
        {
            if (datumConverterFactory == null)
                throw new ArgumentNullException("datumConverterFactory");
            if (rootDatumConverterFactory == null)
                throw new ArgumentNullException("rootDatumConverterFactory");
            IDatumConverter retval;
            if (datumConverterFactory.TryGet(datumType, rootDatumConverterFactory, out retval))
                return retval;
            else
                throw new NotSupportedException(String.Format("Datum converter is not available for type {0}", datumType));
        }

        #endregion
    }
}
