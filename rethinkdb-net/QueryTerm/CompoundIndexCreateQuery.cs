using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RethinkDb.QueryTerm
{
    public class CompoundIndexCreateQuery<TTable> : IndexCreateQuery<TTable, object[]>
    {
        public CompoundIndexCreateQuery(ITableQuery<TTable> tableTerm, string indexName, Expression<Func<TTable, object[]>> indexExpression) 
            : base(tableTerm, indexName, indexExpression, false)
        {
        }

        public CompoundIndexCreateQuery(ITableQuery<TTable> tableTerm, string indexName, Expression[] accessorExpressions)
            : base(tableTerm, indexName, ConvertExpression(accessorExpressions), false)
        {
        }

        internal class ParameterReplacingVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;

            public ParameterReplacingVisitor(ParameterExpression parameter)
            {
                _parameter = parameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parameter;
            }
        }

        private static Expression<Func<TTable, object[]>> ConvertExpression(Expression[] accessorExpressions)
        {
            var param = Expression.Parameter(typeof(TTable));
            var visitor = new ParameterReplacingVisitor(param);

            return Expression.Lambda<Func<TTable, object[]>>(Expression.NewArrayInit(typeof(object),
                accessorExpressions.Select(expr => Expression.Convert(visitor.Visit(((LambdaExpression)expr).Body), typeof(object)))),
                param);
        }
    }
}
