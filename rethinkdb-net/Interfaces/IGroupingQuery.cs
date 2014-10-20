using System;
using System.Collections.Generic;
using RethinkDb.Spec;

namespace RethinkDb
{
    public interface IGroupingQuery<TKey, TValue> : IScalarQuery<IDictionary<TKey, TValue>>
    {
    }
}
