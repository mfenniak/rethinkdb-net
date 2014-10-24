using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class MapGroupQuery<TKey, TOriginal, TTarget> : IGroupingQuery<TKey, TTarget[]>
    {
        private readonly IGroupingQuery<TKey, TOriginal[]> groupingQuery;
        private readonly Expression<Func<TOriginal, TTarget>> mapExpression;

        public MapGroupQuery(IGroupingQuery<TKey, TOriginal[]> groupingQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            this.groupingQuery = groupingQuery;
            this.mapExpression = mapExpression;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var mapTerm = new Term()
            {
                type = Term.TermType.MAP,
            };
            mapTerm.args.Add(groupingQuery.GenerateTerm(queryConverter));
            mapTerm.args.Add(ExpressionUtils.CreateFunctionTerm<TOriginal, TTarget>(queryConverter, mapExpression));
            return mapTerm;
        }
    }
}
