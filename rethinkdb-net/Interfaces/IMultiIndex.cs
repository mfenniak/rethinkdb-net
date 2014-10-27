using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IMultiIndex<TRecord, TIndex> : IBaseIndex<TRecord, TIndex>
    {
        Expression<Func<TRecord, IEnumerable<TIndex>>> IndexAccessor { get; }
    }
}
