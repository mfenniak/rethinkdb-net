using System;
using System.Linq.Expressions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class ReduceGroupQuery<TKey, TRecord> : IGroupingQuery<TKey, TRecord>
    {
        private readonly IGroupingQuery<TKey, TRecord[]> groupingQuery;
        private readonly Expression<Func<TRecord, TRecord, TRecord>> reduceFunction;

        public ReduceGroupQuery(IGroupingQuery<TKey, TRecord[]> groupingQuery, Expression<Func<TRecord, TRecord, TRecord>> reduceFunction)
        {
            this.groupingQuery = groupingQuery;
            this.reduceFunction = reduceFunction;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var reduceTerm = new Term()
            {
                type = Term.TermType.REDUCE,
            };
            reduceTerm.args.Add(groupingQuery.GenerateTerm(queryConverter));
            reduceTerm.args.Add(ExpressionUtils.CreateFunctionTerm<TRecord, TRecord, TRecord>(queryConverter, reduceFunction));
            return reduceTerm;
        }
    }
}
