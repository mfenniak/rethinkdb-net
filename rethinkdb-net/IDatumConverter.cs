using RethinkDb.Spec;
using System.Reflection;

namespace RethinkDb
{
    public interface IDatumConverter<T>
    {
        T ConvertDatum(Datum datum);
        Datum ConvertObject(T @object);
    }

    public interface IObjectDatumConverter
    {
        string GetDatumFieldName(MemberInfo memberInfo);
    }
}
