using RethinkDb.Spec;

namespace RethinkDb
{
    public interface IDatumConverter<T>
    {
        T ConvertDatum(Datum datum);
        Datum ConvertObject(T @object);
    }
}
