using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class LimitQueryChangefeedCompatible<T> : LimitQuery<T>, IChangefeedCompatibleQuery<T>
    {
        public LimitQueryChangefeedCompatible(IOrderByIndexQuery<T> sequenceQuery, int skipCount)
            : base(sequenceQuery, skipCount)
        {
        }
    }
}
