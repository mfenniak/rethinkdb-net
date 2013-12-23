using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class FilterQuery<T> : ISequenceQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly Expression<Func<T, bool>> filterExpression;

        public FilterQuery(ISequenceQuery<T> sequenceQuery, Expression<Func<T, bool>> filterExpression)
        {
            this.sequenceQuery = sequenceQuery;
            this.filterExpression = filterExpression;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var filterTerm = new Term()
            {
                type = Term.TermType.FILTER,
            };
            filterTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));

            if (filterExpression.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type");
            filterTerm.args.Add(ExpressionUtils.CreateFunctionTerm<T, bool>(datumConverterFactory, expressionConverterFactory, filterExpression));

            return filterTerm;
        }
    }
}
