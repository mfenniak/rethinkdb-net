using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DeleteAndReturnValueQuery<T> : DeleteQueryBase<T>, IWriteQuery<DmlResponse<T>>
    {
        public DeleteAndReturnValueQuery(IMutableSingleObjectQuery<T> getTerm)
            : base(getTerm)
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

