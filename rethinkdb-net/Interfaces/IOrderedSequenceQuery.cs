using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IOrderedSequenceQuery<T> : ISequenceQuery<T>
    {
        ISequenceQuery<T> SequenceQuery { get; }
        IEnumerable<Tuple<Expression<Func<T, object>>, OrderByDirection>> OrderByMembers { get; }
    }
}

