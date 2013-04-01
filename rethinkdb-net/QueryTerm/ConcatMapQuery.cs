using RethinkDb.Spec;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace RethinkDb.QueryTerm
{
    public class ConcatMapQuery<TOriginal, TTarget> : ISequenceQuery<TTarget>
    {
        private readonly ISequenceQuery<TOriginal> sequenceQuery;
        private readonly Expression<Func<TOriginal, IEnumerable<TTarget>>> mapExpression;

        public ConcatMapQuery(ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, IEnumerable<TTarget>>> mapExpression)
        {
            this.sequenceQuery = sequenceQuery;
            this.mapExpression = mapExpression;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var mapTerm = new Term()
            {
                type = Term.TermType.CONCATMAP,
            };
            mapTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));
            mapTerm.args.Add(ExpressionUtils.CreateFunctionTerm<TOriginal, IEnumerable<TTarget>>(datumConverterFactory, mapExpression));
            return mapTerm;
        }
    }
}
