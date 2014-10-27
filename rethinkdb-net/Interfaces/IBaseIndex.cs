using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IBaseIndex<TRecord, TIndex>
    {
        ITableQuery<TRecord> Table { get; }
        string Name { get; }
    }
}
