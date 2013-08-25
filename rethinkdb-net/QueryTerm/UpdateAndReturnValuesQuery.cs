using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class UpdateAndReturnValuesQuery<T> : UpdateQueryBase<T>, IWriteQuery<T, DmlResponse<T>>
    {
        public UpdateAndReturnValuesQuery(ISequenceQuery<T> tableTerm, Expression<Func<T, T>> updateExpression, bool nonAtomic)
            : base(tableTerm, updateExpression, nonAtomic)
        {
        }

        public UpdateAndReturnValuesQuery(IMutableSingleObjectQuery<T> singleObjectTerm, Expression<Func<T, T>> updateExpression, bool nonAtomic)
            : base(singleObjectTerm, updateExpression, nonAtomic)
        {
        }

        protected override void AddOptionalArguments(Term updateTerm)
        {
            updateTerm.optargs.Add(new Term.AssocPair() {
                key = "return_vals",
                val = new Term() {
                    type = Term.TermType.DATUM,
                    datum = new Datum() {
                        type = Datum.DatumType.R_BOOL,
                        r_bool = true
                    }
                }
            });
        }
    }
}
