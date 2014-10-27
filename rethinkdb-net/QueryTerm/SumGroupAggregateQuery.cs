using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class SumGroupAggregateQuery<TKey, TRecord, TSumType> : IGroupingQuery<TKey, TSumType>
    {
        private readonly IGroupingQuery<TKey, TRecord[]> groupingQuery;
        private readonly Expression<Func<TRecord, TSumType>> field;

        public SumGroupAggregateQuery(IGroupingQuery<TKey, TRecord[]> groupingQuery, Expression<Func<TRecord, TSumType>> field)
        {
            this.groupingQuery = groupingQuery;
            this.field = field;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.SUM,
            };
            term.args.Add(groupingQuery.GenerateTerm(queryConverter));
            if (field != null)
            {
                if (field.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type");
                term.args.Add(ExpressionUtils.CreateFunctionTerm<TRecord, TSumType>(queryConverter, field));
            }
            return term;
        }
    }
}