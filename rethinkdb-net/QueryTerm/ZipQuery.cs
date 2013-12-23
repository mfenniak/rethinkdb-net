using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class ZipQuery<TLeft, TRight, TTarget> : ISequenceQuery<TTarget>
    {
        private ISequenceQuery<Tuple<TLeft, TRight>> sequenceQuery;

        public ZipQuery(ISequenceQuery<Tuple<TLeft, TRight>> sequenceQuery)
        {
            this.sequenceQuery = sequenceQuery;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var zipTerm = new Term()
            {
                type = Term.TermType.ZIP,
            };
            zipTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            return zipTerm;
        }
    }
}
