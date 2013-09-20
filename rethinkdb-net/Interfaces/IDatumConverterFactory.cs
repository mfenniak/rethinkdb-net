using System;

namespace RethinkDb
{
    public interface IDatumConverterFactory
    {
        bool TryGet(Type datumType, IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter datumConverter);
        bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter);
    }
}
