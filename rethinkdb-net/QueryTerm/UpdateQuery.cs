using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class UpdateQuery<T> : IWriteQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceTerm;
        private readonly IMutableSingleObjectQuery<T> singleObjectTerm;
        private readonly Expression<Func<T, T>> updateExpression;

        public UpdateQuery(ISequenceQuery<T> tableTerm, Expression<Func<T, T>> updateExpression)
        {
            this.sequenceTerm = tableTerm;
            this.updateExpression = updateExpression;
        }

        public UpdateQuery(IMutableSingleObjectQuery<T> singleObjectTerm, Expression<Func<T, T>> updateExpression)
        {
            this.singleObjectTerm = singleObjectTerm;
            this.updateExpression = updateExpression;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var updateTerm = new Term()
            {
                type = Term.TermType.UPDATE,
            };
            if (singleObjectTerm != null)
                updateTerm.args.Add(singleObjectTerm.GenerateTerm(datumConverterFactory));
            else
                updateTerm.args.Add(sequenceTerm.GenerateTerm(datumConverterFactory));

            var body = updateExpression.Body;
            if (body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException("Only MemberInit expression type is currently supported in Update");

            updateTerm.args.Add(ExpressionUtils.CreateFunctionTerm<T, T>(datumConverterFactory, updateExpression));

            return updateTerm;
        }
    }
}
