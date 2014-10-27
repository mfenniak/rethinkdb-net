using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class SumAggregateQuery<TRecord, TSumType> : ISingleObjectQuery<TSumType>
    {
        private readonly ISequenceQuery<TRecord> sequenceQuery;
        private readonly Expression<Func<TRecord, TSumType>> field;

        public SumAggregateQuery(ISequenceQuery<TRecord> sequenceQuery, Expression<Func<TRecord, TSumType>> field)
        {
            this.sequenceQuery = sequenceQuery;
            this.field = field;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.SUM,
            };
            term.args.Add(sequenceQuery.GenerateTerm(queryConverter));
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