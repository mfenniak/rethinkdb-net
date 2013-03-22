using RethinkDb.Spec;
using System.Linq.Expressions;
using System;

namespace RethinkDb.QueryTerm
{
    public class GetQuery<T> : ISingleObjectQuery<T>
    {
        private readonly TableQuery<T> tableTerm;
        private readonly string primaryKey;
        private readonly string primaryAttribute;

        public GetQuery(TableQuery<T> tableTerm, string primaryKey, string primaryAttribute)
        {
            this.tableTerm = tableTerm;
            this.primaryKey = primaryKey;
            this.primaryAttribute = primaryAttribute;
        }

        public ReplaceQuery<T> Replace(T newObject)
        {
            return new ReplaceQuery<T>(this, newObject);
        }

        public DeleteQuery<T> Delete()
        {
            return new DeleteQuery<T>(this);
        }

        Spec.Term ISingleObjectQuery<T>.GenerateTerm()
        {
            var getTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.GET,
            };
            getTerm.args.Add(((ISequenceQuery<T>)tableTerm).GenerateTerm());
            getTerm.args.Add(
                new Spec.Term()
                {
                    type = Spec.Term.TermType.DATUM,
                    datum = new Spec.Datum()
                    {
                        type = Spec.Datum.DatumType.R_STR,
                        r_str = primaryKey,
                    }
                }
            );
            if (primaryAttribute != null)
            {
                getTerm.optargs.Add(new Spec.Term.AssocPair()
                {
                    key = "attribute",
                    val = new Spec.Term()
                    {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_STR,
                            r_str = primaryAttribute,
                        }
                    }
                });
            }
            return getTerm;
        }
    }
}
