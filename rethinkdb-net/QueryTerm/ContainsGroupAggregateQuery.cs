using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class ContainsGroupAggregateQuery<TKey, TRecord> : IGroupingQuery<TKey, bool>
    {
        private readonly IGroupingQuery<TKey, TRecord[]> groupingQuery;
        private readonly Expression<Func<TRecord, bool>> predicate;

        public ContainsGroupAggregateQuery(IGroupingQuery<TKey, TRecord[]> groupingQuery, Expression<Func<TRecord, bool>> predicate)
        {
            this.groupingQuery = groupingQuery;
            this.predicate = predicate;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.CONTAINS,
            };
            term.args.Add(groupingQuery.GenerateTerm(datumConverterFactory));
            if (predicate != null)
            {
                if (predicate.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type");
                term.args.Add(ExpressionUtils.CreateFunctionTerm<TRecord, bool>(datumConverterFactory, predicate));
            }
            return term;
        }
    }
}
