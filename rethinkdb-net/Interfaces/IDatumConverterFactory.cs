namespace RethinkDb
{
    public interface IDatumConverterFactory
    {
        bool TryGet<T>(IDatumConverterFactory rootDatumConverterFactory, out IDatumConverter<T> datumConverter);
    }
}
