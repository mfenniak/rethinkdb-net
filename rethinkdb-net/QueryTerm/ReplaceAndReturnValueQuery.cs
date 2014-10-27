using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class ReplaceAndReturnValueQuery<T> : ReplaceQueryBase<T>, IWriteQuery<DmlResponse<T>>
    {
        public ReplaceAndReturnValueQuery(IMutableSingleObjectQuery<T> getTerm, T newObject, bool nonAtomic)
            : base(getTerm, newObject, nonAtomic)
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

