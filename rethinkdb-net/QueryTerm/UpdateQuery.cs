using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class UpdateQuery<T> : UpdateQueryBase<T>, IWriteQuery<T, DmlResponse>
    {
        public UpdateQuery(ISequenceQuery<T> tableTerm, Expression<Func<T, T>> updateExpression, bool nonAtomic)
            : base(tableTerm, updateExpression, nonAtomic)
        {
        }

        public UpdateQuery(IMutableSingleObjectQuery<T> singleObjectTerm, Expression<Func<T, T>> updateExpression, bool nonAtomic)
            : base(singleObjectTerm, updateExpression, nonAtomic)
        {
        }
    }
}
