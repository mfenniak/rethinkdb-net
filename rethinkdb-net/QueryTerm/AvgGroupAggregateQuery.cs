using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class AvgGroupAggregateQuery<TKey, TRecord> : IGroupingQuery<TKey, double>
    {
        private readonly IGroupingQuery<TKey, TRecord[]> groupingQuery;
        private readonly Expression<Func<TRecord, double>> field;

        public AvgGroupAggregateQuery(IGroupingQuery<TKey, TRecord[]> groupingQuery, Expression<Func<TRecord, double>> field)
        {
            this.groupingQuery = groupingQuery;
            this.field = field;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.AVG,
            };
            term.args.Add(groupingQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            if (field != null)
            {
                if (field.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type");
                term.args.Add(ExpressionUtils.CreateFunctionTerm<TRecord, double>(datumConverterFactory, expressionConverterFactory, field));
            }
            return term;
        }
    }
}