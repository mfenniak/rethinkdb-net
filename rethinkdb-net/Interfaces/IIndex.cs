using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IIndex<TRecord, TIndex> : IBaseIndex<TRecord, TIndex>
    {
        Expression<Func<TRecord, TIndex>> IndexAccessor { get; }
    }
}
