using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class MaxGroupAggregateQuery<TKey, TRecord, TExpressionValue> : IGroupingQuery<TKey, TRecord>
    {
        private readonly IGroupingQuery<TKey, TRecord[]> groupingQuery;
        private readonly Expression<Func<TRecord, TExpressionValue>> field;

        public MaxGroupAggregateQuery(IGroupingQuery<TKey, TRecord[]> groupingQuery, Expression<Func<TRecord, TExpressionValue>> field)
        {
            this.groupingQuery = groupingQuery;
            this.field = field;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.MAX,
            };
            term.args.Add(groupingQuery.GenerateTerm(datumConverterFactory));
            if (field != null)
            {
                if (field.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type");
                term.args.Add(ExpressionUtils.CreateFunctionTerm<TRecord, TExpressionValue>(datumConverterFactory, field));
            }
            return term;
        }
    }
}