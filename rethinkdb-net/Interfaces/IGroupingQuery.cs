using System;

namespace RethinkDb
{
    public interface IGroupingQuery<TKey, TValue> : IScalarQuery<IGroupingDictionary<TKey, TValue>>
    {
    }
}
