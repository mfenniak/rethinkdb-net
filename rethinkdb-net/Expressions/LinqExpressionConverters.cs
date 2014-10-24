using System;
using RethinkDb.Spec;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using RethinkDb.QueryTerm;
using System.Reflection;

namespace RethinkDb.Expressions
{
    public static class LinqExpressionConverters
    {
        private delegate T[] AppendDelegate<T>(T[] array, params T[] additionalObjects);
        private delegate IEnumerable<T> WhereDelegate<T>(IEnumerable<T> source,Func<T, bool> predicate);
        private delegate int CountDelegate<T>(IEnumerable<T> source);

        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            var appendDelegate = (AppendDelegate<int>)ReQLExpression.Append;
            expressionConverterFactory.RegisterMethodCallMapping(appendDelegate.Method, ConvertAppendToTerm);

            var whereDelegate = (WhereDelegate<int>)Enumerable.Where;
            expressionConverterFactory.RegisterMethodCallMapping(whereDelegate.Method, ConvertEnumerableWhereToTerm);

            var countDelegate = (CountDelegate<int>)Enumerable.Count;
            expressionConverterFactory.RegisterMethodCallMapping(countDelegate.Method, ConvertEnumerableCountToTerm);
        }

        public static Term ConvertAppendToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var target = methodCall.Arguments[0];

            var appendArray = methodCall.Arguments[1];
            if (appendArray.NodeType != ExpressionType.NewArrayInit)
                throw new NotSupportedException(String.Format("Expected second arg to ReQLExpression.Append to be NewArrayInit, but was: {0}", appendArray.NodeType));

            var newArrayExpression = (NewArrayExpression)appendArray;
            var term = recursiveMap(target);
            foreach (var datumExpression in newArrayExpression.Expressions)
            {
                var newTerm = new Term() {
                    type = Term.TermType.APPEND
                };
                newTerm.args.Add(term);

                if (datumExpression.NodeType == ExpressionType.MemberInit)
                {
                    var memberInit = (MemberInitExpression)datumExpression;
                    newTerm.args.Add(recursiveMap(memberInit));
                }
                else
                    throw new NotSupportedException(String.Format("Expected ReQLExpression.Append to contain MemberInit additions, but was: {0}", datumExpression.NodeType));

                term = newTerm;
            }

            return term;
        }

        public static Term ConvertEnumerableWhereToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var target = methodCall.Arguments[0];
            var predicate = methodCall.Arguments[1];

            var filterTerm = new Term()
            {
                type = Term.TermType.FILTER
            };

            filterTerm.args.Add(recursiveMap(target));

            var enumerableElementType = methodCall.Method.ReturnType.GetGenericArguments()[0];
            var createFunctionTermMethod = typeof(ExpressionUtils)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Single(m => m.Name == "CreateFunctionTerm" && m.GetGenericArguments().Length == 2);
            createFunctionTermMethod = createFunctionTermMethod.MakeGenericMethod(enumerableElementType, typeof(bool));

            var functionTerm = (Term)createFunctionTermMethod.Invoke(null, new object[] { datumConverterFactory, expressionConverterFactory, predicate });
            filterTerm.args.Add(functionTerm);

            return filterTerm;
        }

        public static Term ConvertEnumerableCountToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var target = methodCall.Arguments[0];

            var countTerm = new Term()
            {
                type = Term.TermType.COUNT,
            };
            countTerm.args.Add(recursiveMap(target));
            return countTerm;
        }
    }
}
