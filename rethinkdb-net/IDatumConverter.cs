using RethinkDb.Spec;

namespace RethinkDb
{
    interface IDatumConverter<T>
    {
        T ConvertDatum(Datum datum);
    }
}
