using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class UpdateAndReturnValueQuery<T> : UpdateQueryBase<T>, IWriteQuery<DmlResponse<T>>
    {
        public UpdateAndReturnValueQuery(IMutableSingleObjectQuery<T> singleObjectTerm, Expression<Func<T, T>> updateExpression, bool nonAtomic)
            : base(singleObjectTerm, updateExpression, nonAtomic)
        {
        }

        protected override void AddOptionalArguments(Term updateTerm)
        {
            updateTerm.optargs.Add(new Term.AssocPair() {
                key = "return_changes",
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
