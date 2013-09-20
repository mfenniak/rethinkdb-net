using RethinkDb.Spec;
using System.Reflection;

namespace RethinkDb
{
    public interface IDatumConverter
    {
        object ConvertDatum(Datum datum);
        Datum ConvertObject(object @object);
    }

    public interface IDatumConverter<T> : IDatumConverter
    {
        new T ConvertDatum(Datum datum);
        Datum ConvertObject(T @object);
    }
}
