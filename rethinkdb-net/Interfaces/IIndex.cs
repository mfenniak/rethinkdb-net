using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IIndex<TRecord, TIndex>
    {
        ITableQuery<TRecord> Table { get; }
        string Name { get; }
        Expression<Func<TRecord, TIndex>> IndexAccessor { get; }
    }
}
