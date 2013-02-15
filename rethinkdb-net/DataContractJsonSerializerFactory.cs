using System;

namespace RethinkDb
{
    class DataContractJsonSerializerFactory : IJsonSerializerFactory
    {
        private static class Cache<TJsonSerializer, TType>
            where TJsonSerializer : IJsonSerializer<TType>
        {
            public static Lazy<IJsonSerializer<TType>> Instance = new Lazy<IJsonSerializer<TType>>(() => new DataContractJsonSerializer<TType>());
        }

        public IJsonSerializer<T> Get<T>()
        {
            return Cache<IJsonSerializer<T>, T>.Instance.Value;
        }
    }
}
