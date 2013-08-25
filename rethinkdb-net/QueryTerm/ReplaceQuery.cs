using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class ReplaceQuery<T> : IWriteQuery<T, DmlResponse>
    {
        private readonly IMutableSingleObjectQuery<T> getTerm;
        private readonly T newObject;
        private readonly bool nonAtomic;

        public ReplaceQuery(IMutableSingleObjectQuery<T> getTerm, T newObject, bool nonAtomic)
        {
            this.getTerm = getTerm;
            this.newObject = newObject;
            this.nonAtomic = nonAtomic;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var replaceTerm = new Term()
            {
                type = Term.TermType.REPLACE,
            };
            replaceTerm.args.Add(getTerm.GenerateTerm(datumConverterFactory));
            replaceTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = datumConverterFactory.Get<T>().ConvertObject(newObject)
            });

            if (nonAtomic)
            {
                replaceTerm.optargs.Add(new Term.AssocPair() {
                    key = "non_atomic",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_BOOL,
                            r_bool = nonAtomic
                        }
                    }
                });
            }

            return replaceTerm;
        }
    }
}

