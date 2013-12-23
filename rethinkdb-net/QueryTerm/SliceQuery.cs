using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class SliceQuery<T> : ISequenceQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly int startIndex;
        private readonly int? endIndex;

        public SliceQuery(ISequenceQuery<T> sequenceQuery, int startIndex, int? endIndex)
        {
            this.sequenceQuery = sequenceQuery;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.SLICE,
            };
            term.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            term.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum()
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = startIndex,
                }
            });
            term.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum()
                {
                    type = Datum.DatumType.R_NUM,
                    r_num = endIndex.GetValueOrDefault(int.MaxValue),
                }
            });
            return term;
        }
    }
}