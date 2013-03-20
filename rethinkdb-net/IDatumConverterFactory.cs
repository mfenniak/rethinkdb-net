namespace RethinkDb
{
    public interface IDatumConverterFactory
    {
        IDatumConverter<T> Get<T>();
    }
}
