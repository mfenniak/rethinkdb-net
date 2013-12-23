using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class LimitQuery<T> : ISequenceQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly int skipCount;

        public LimitQuery(ISequenceQuery<T> sequenceQuery, int skipCount)
        {
            this.sequenceQuery = sequenceQuery;
            this.skipCount = skipCount;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.LIMIT,
            };
            term.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            term.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum()
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = skipCount,
                }
            });
            return term;
        }
    }
}