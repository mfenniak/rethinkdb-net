namespace RethinkDb
{
    interface IDatumConverterFactory
    {
        IDatumConverter<T> Get<T>();
    }
}
