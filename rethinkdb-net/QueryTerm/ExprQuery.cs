using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class ExprQuery<T> : ISingleObjectQuery<T>
    {
        private readonly T @object;
        private readonly Expression<Func<T>> objectExpr;

        public ExprQuery(T @object)
        {
            this.@object = @object;
        }

        public ExprQuery(Expression<Func<T>> objectExpr)
        {
            this.objectExpr = objectExpr;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            if (objectExpr != null)
            {
                if (objectExpr.NodeType != ExpressionType.Lambda)
                    throw new NotSupportedException("Unsupported expression type");

                var body = objectExpr.Body;
                return ExpressionUtils.MapExpressionToTerm<T>(datumConverterFactory, body);
            }
            else
            {
                var datumTerm = new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = datumConverterFactory.Get<T>().ConvertObject(@object)
                };
                return datumTerm;
            }
        }
    }
}

