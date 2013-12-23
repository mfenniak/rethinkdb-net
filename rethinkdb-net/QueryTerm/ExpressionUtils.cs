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
        public static Term CreateValueTerm<TReturn>(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory, Expression<Func<TReturn>> expression)
        {
            var termConverter = expressionConverterFactory.CreateExpressionConverter<TReturn>(datumConverterFactory);
            return termConverter.CreateFunctionTerm(expression);
        }

        public static Term CreateFunctionTerm<TParameter1, TReturn>(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory, Expression<Func<TParameter1, TReturn>> expression)
        {
            var termConverter = expressionConverterFactory.CreateExpressionConverter<TParameter1, TReturn>(datumConverterFactory);
            return termConverter.CreateFunctionTerm(expression);
        }

        public static Term CreateFunctionTerm<TParameter1, TParameter2, TReturn>(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory, Expression<Func<TParameter1, TParameter2, TReturn>> expression)
        {
            var termConverter = expressionConverterFactory.CreateExpressionConverter<TParameter1, TParameter2, TReturn>(datumConverterFactory);
            return termConverter.CreateFunctionTerm(expression);
        }
    }
}

