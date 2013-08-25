using RethinkDb.Spec;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RethinkDb.Expressions;

namespace RethinkDb.QueryTerm
{
    static class ExpressionUtils
    {
        public static Term CreateValueTerm<TReturn>(IDatumConverterFactory datumConverterFactory, Expression<Func<TReturn>> expression)
        {
            var converter = new ZeroParameterLambda<TReturn>(datumConverterFactory);
            return converter.CreateFunctionTerm(expression);
        }

        public static Term CreateFunctionTerm<TParameter1, TReturn>(IDatumConverterFactory datumConverterFactory, Expression<Func<TParameter1, TReturn>> expression)
        {
            var converter = new SingleParameterLambda<TParameter1, TReturn>(datumConverterFactory);
            return converter.CreateFunctionTerm(expression);
        }

        public static Term CreateFunctionTerm<TParameter1, TParameter2, TReturn>(IDatumConverterFactory datumConverterFactory, Expression<Func<TParameter1, TParameter2, TReturn>> expression)
        {
            var converter = new TwoParameterLambda<TParameter1, TParameter2, TReturn>(datumConverterFactory);
            return converter.CreateFunctionTerm(expression);
        }
    }
}

