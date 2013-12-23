using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class NthQuery<T> : ISingleObjectQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly int index;

        public NthQuery(ISequenceQuery<T> sequenceQuery, int index)
        {
            this.sequenceQuery = sequenceQuery;
            this.index = index;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.NTH,
            };
            term.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            term.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum()
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = index,
                }
            });
            return term;
        }
    }
}