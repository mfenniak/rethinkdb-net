namespace RethinkDb
{
    interface IJsonSerializer<T>
    {
        T Deserialize(Datum datum);
    }
}
