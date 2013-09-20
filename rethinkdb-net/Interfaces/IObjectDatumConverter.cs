using System.Reflection;

namespace RethinkDb
{
    public interface IObjectDatumConverter
    {
        string GetDatumFieldName(MemberInfo memberInfo);
    }
}

