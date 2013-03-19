using System;

namespace RethinkDb
{
    class DataContractDatumConverterFactory : IDatumConverterFactory
    {
        private static class Cache<TType>
        {
            public static Lazy<IDatumConverter<TType>> Instance = new Lazy<IDatumConverter<TType>>(() => new DataContractDatumConverter<TType>());
        }

        public IDatumConverter<T> Get<T>()
        {
            return Cache<T>.Instance.Value;
        }
    }
}
