namespace RethinkDb
{
    interface IJsonSerializerFactory
    {
        IJsonSerializer<T> Get<T>();
    }
}
