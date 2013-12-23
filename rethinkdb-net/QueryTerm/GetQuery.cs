using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class GetQuery<T> : IMutableSingleObjectQuery<T>
    {
        private readonly ISequenceQuery<T> tableTerm;
        private readonly string primaryKeyString;
        private readonly double? primaryKeyNumeric;
        private readonly string primaryAttribute;

        public GetQuery(ISequenceQuery<T> tableTerm, string primaryKeyString, string primaryAttribute)
        {
            this.tableTerm = tableTerm;
            this.primaryKeyString = primaryKeyString;
            this.primaryAttribute = primaryAttribute;
        }

        public GetQuery(ISequenceQuery<T> tableTerm, double primaryKeyNumeric, string primaryAttribute)
        {
            this.tableTerm = tableTerm;
            this.primaryKeyNumeric = primaryKeyNumeric;
            this.primaryAttribute = primaryAttribute;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var getTerm = new Term()
            {
                type = Term.TermType.GET,
            };
            getTerm.args.Add(tableTerm.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            if (primaryKeyNumeric.HasValue)
            {
                getTerm.args.Add(
                    new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = primaryKeyNumeric.Value,
                        }
                    }
                );
            }
            else
            {
                getTerm.args.Add(
                    new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = primaryKeyString,
                        }
                    }
                );
            }
            if (!String.IsNullOrEmpty(primaryAttribute))
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
