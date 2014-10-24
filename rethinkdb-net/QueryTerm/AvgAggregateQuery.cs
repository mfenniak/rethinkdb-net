using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class AvgAggregateQuery<TRecord> : ISingleObjectQuery<double>
    {
        private readonly ISequenceQuery<TRecord> sequenceQuery;
        private readonly Expression<Func<TRecord, double>> field;

        public AvgAggregateQuery(ISequenceQuery<TRecord> sequenceQuery, Expression<Func<TRecord, double>> field)
        {
            this.sequenceQuery = sequenceQuery;
            this.field = field;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.AVG,
            };
            term.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
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