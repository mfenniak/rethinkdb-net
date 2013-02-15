namespace RethinkDb
{
    interface IJsonSerializer<T>
    {
        T Deserialize(string jsonText);
    }
}
