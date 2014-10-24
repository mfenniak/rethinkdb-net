using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class CountAggregateQuery<T> : ISingleObjectQuery<int>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly Expression<Func<T, bool>> predicate;

        public CountAggregateQuery(ISequenceQuery<T> sequenceQuery, Expression<Func<T, bool>> predicate = null)
        {
            this.sequenceQuery = sequenceQuery;
            this.predicate = predicate;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var countTerm = new Term()
            {
                type = Term.TermType.COUNT,
            };
            countTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            if (predicate != null)
            {
                if (predicate.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type");
                countTerm.args.Add(ExpressionUtils.CreateFunctionTerm<T, bool>(datumConverterFactory, expressionConverterFactory, predicate));
            }
            return countTerm;
        }
    }
}
