using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class GetQuery<T> : ISingleObjectQuery<T>, IMutableSingleObjectQuery<T>
    {
        private readonly ISequenceQuery<T> tableTerm;
        private readonly string primaryKey;
        private readonly string primaryAttribute;

        public GetQuery(ISequenceQuery<T> tableTerm, string primaryKey, string primaryAttribute)
        {
            this.tableTerm = tableTerm;
            this.primaryKey = primaryKey;
            this.primaryAttribute = primaryAttribute;
        }

        public Term GenerateTerm()
        {
            var getTerm = new Term()
            {
                type = Term.TermType.GET,
            };
            getTerm.args.Add(tableTerm.GenerateTerm());
            getTerm.args.Add(
                new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = primaryKey,
                    }
                }
            );
            if (primaryAttribute != null)
            {
                getTerm.optargs.Add(new Term.AssocPair()
                {
                    key = "attribute",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = primaryAttribute,
                        }
                    }
                });
            }
            return getTerm;
        }
    }
}
