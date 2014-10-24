using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class MinAggregateQuery<TRecord, TExpressionValue> : ISingleObjectQuery<TRecord>
    {
        private readonly ISequenceQuery<TRecord> sequenceQuery;
        private readonly Expression<Func<TRecord, TExpressionValue>> field;

        public MinAggregateQuery(ISequenceQuery<TRecord> sequenceQuery, Expression<Func<TRecord, TExpressionValue>> field)
        {
            this.sequenceQuery = sequenceQuery;
            this.field = field;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.MIN,
            };
            term.args.Add(sequenceQuery.GenerateTerm(queryConverter));
            if (field != null)
            {
                if (field.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type");
                term.args.Add(ExpressionUtils.CreateFunctionTerm<TRecord, TExpressionValue>(queryConverter, field));
            }
            return term;
        }
    }
}