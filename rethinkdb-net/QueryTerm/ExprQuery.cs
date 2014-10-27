using System;
using System.Linq.Expressions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

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

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            if (objectExpr != null)
            {
                return ExpressionUtils.CreateValueTerm<T>(queryConverter, objectExpr);
            }
            else
            {
                var datumTerm = new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = queryConverter.Get<T>().ConvertObject(@object)
                };
                return datumTerm;
            }
        }
    }
}

