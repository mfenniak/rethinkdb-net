using RethinkDb.Spec;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RethinkDb.Expressions;

namespace RethinkDb.QueryTerm
{
    public static class ExpressionUtils
    {
        public static Term CreateValueTerm<TReturn>(IQueryConverter queryConverter, Expression<Func<TReturn>> expression)
        {
            var termConverter = queryConverter.CreateExpressionConverter<TReturn>(queryConverter);
            return termConverter.CreateFunctionTerm(expression);
        }

        public static Term CreateFunctionTerm<TParameter1, TReturn>(IQueryConverter queryConverter, Expression<Func<TParameter1, TReturn>> expression)
        {
            var termConverter = queryConverter.CreateExpressionConverter<TParameter1, TReturn>(queryConverter);
            return termConverter.CreateFunctionTerm(expression);
        }

        public static Term CreateFunctionTerm<TParameter1, TParameter2, TReturn>(IQueryConverter queryConverter, Expression<Func<TParameter1, TParameter2, TReturn>> expression)
        {
            var termConverter = queryConverter.CreateExpressionConverter<TParameter1, TParameter2, TReturn>(queryConverter);
            return termConverter.CreateFunctionTerm(expression);
        }
    }
}
