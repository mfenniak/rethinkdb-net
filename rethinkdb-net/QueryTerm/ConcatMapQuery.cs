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

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var mapTerm = new Term()
            {
                type = Term.TermType.CONCAT_MAP,
            };
            mapTerm.args.Add(sequenceQuery.GenerateTerm(queryConverter));
            mapTerm.args.Add(ExpressionUtils.CreateFunctionTerm<TOriginal, IEnumerable<TTarget>>(queryConverter, mapExpression));
            return mapTerm;
        }
    }
}
