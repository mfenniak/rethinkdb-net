using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public static class LinqExpressionConverters
    {
        private delegate T[] AppendDelegate<T>(T[] array, params T[] additionalObjects);
        private delegate IEnumerable<T> WhereDelegate<T>(IEnumerable<T> source, Func<T, bool> predicate);
        private delegate bool AnyDelegate<T>(IEnumerable<T> source, Func<T, bool> predicate);
        private delegate int CountDelegate<T>(IEnumerable<T> source);

        public static void RegisterOnConverterFactory(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            var appendDelegate = (AppendDelegate<int>)ReQLExpression.Append;
            expressionConverterFactory.RegisterMethodCallMapping(appendDelegate.Method, ConvertAppendToTerm);

            var whereDelegate = (WhereDelegate<int>)Enumerable.Where;
            expressionConverterFactory.RegisterMethodCallMapping(whereDelegate.Method, ConvertEnumerableWhereToTerm);

            var anyDelegate = (AnyDelegate<int>)Enumerable.Any;
            expressionConverterFactory.RegisterMethodCallMapping(anyDelegate.Method, ConvertEnumerableAnyToTerm);

            expressionConverterFactory.RegisterTemplateMapping<IEnumerable<int>, int, bool>(
                (list, arg) => list.Contains(arg),
                (list, arg) => new Term()
                    {
                    type = Term.TermType.CONTAINS,
                    args = { list, arg }
                });

            expressionConverterFactory.RegisterTemplateMapping<IList<int>, int, bool>(
                (list, arg) => list.Contains(arg),
                (list, arg) => new Term()
                    {
                        type = Term.TermType.CONTAINS,
                        args = { list, arg }
                    });

            expressionConverterFactory.RegisterTemplateMapping<List<int>, int, bool>(
                (list, arg) => list.Contains(arg),
                (list, arg) => new Term()
                    {
                        type = Term.TermType.CONTAINS,
                        args = { list, arg }
                    });

            expressionConverterFactory.RegisterTemplateMapping<IEnumerable<int>, int>(
                list => list.Count(),
                list => Count(list)
            );
            expressionConverterFactory.RegisterTemplateMapping<List<int>, int>(
                list => list.Count,
                list => Count(list)
            );
            expressionConverterFactory.RegisterTemplateMapping<ICollection<int>, int>(
                list => list.Count,
                list => Count(list)
            );

            expressionConverterFactory.RegisterTemplateMapping<int[], int, int[]>(
                (list, startIndex) => ReQLExpression.Slice(list, startIndex),
                (list, startIndex) => new Term()
                {
                    type = Term.TermType.SLICE,
                    args = { list, startIndex }
                });
            expressionConverterFactory.RegisterTemplateMapping<int[], int, int, int[]>(
                (list, startIndex, endIndex) => ReQLExpression.Slice(list, startIndex, endIndex),
                (list, startIndex, endIndex) => new Term()
                {
                    type = Term.TermType.SLICE,
                    args = { list, startIndex, endIndex }
                });
            expressionConverterFactory.RegisterTemplateMapping<int[], int, int, Bound, Bound, int[]>(
                (list, startIndex, endIndex, leftBound, rightBound) => ReQLExpression.Slice(list, startIndex, endIndex, leftBound, rightBound),
                (list, startIndex, endIndex, leftBound, rightBound) => new Term()
                {
                    type = Term.TermType.SLICE,
                    args = { list, startIndex, endIndex },
                    optargs = {
                        new Term.AssocPair()
                        {
                            key = "left_bound",
                            val = leftBound,
                        },
                        new Term.AssocPair()
                        {
                            key = "right_bound",
                            val = rightBound,
                        }
                    }
                });

            expressionConverterFactory.RegisterTemplateMapping<string, int>(
                (errorMessage) => ReQLExpression.Error<int>(errorMessage),
                (errorMessage) => new Term()
                {
                    type = Term.TermType.ERROR,
                    args = { errorMessage }
                });
        }

        private static Term Count(Term term)
        {
            return new Term() { type = Term.TermType.COUNT, args = { term } };
        }

        public static Term ConvertAppendToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var target = methodCall.Arguments[0];

            var appendArray = methodCall.Arguments[1];

            IEnumerable<Expression> appendExpressions;
            if (appendArray.NodeType == ExpressionType.NewArrayInit)
            {
                appendExpressions = ((NewArrayExpression)appendArray).Expressions;
            }
            else if (appendArray.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)appendArray;
                if (!memberExpression.Type.IsArray)
                    throw new NotSupportedException(String.Format("Expected second arg to ReQLExpression.Append to be an array, but was: {0}", memberExpression.Type));

                var array = (IEnumerable)Expression.Lambda(memberExpression).Compile().DynamicInvoke();
                var items = array as object[] ?? array.Cast<object>().ToArray();

                appendExpressions = items.Select(item => Expression.Constant(item));
            }
            else
            {
                throw new NotSupportedException(String.Format("Expected second arg to ReQLExpression.Append to be NewArrayInit or MemberAccess, but was: {0}", appendArray.NodeType));
            }
            
            var term = recursiveMap(target);
            foreach (var datumExpression in appendExpressions)
            {
                var newTerm = new Term() {
                    type = Term.TermType.APPEND
                };
                newTerm.args.Add(term);
                newTerm.args.Add(recursiveMap(datumExpression));

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

            var functionTerm = (Term)createFunctionTermMethod.Invoke(null, new object[] { new QueryConverter(datumConverterFactory, expressionConverterFactory), predicate });
            filterTerm.args.Add(functionTerm);

            return filterTerm;
        }

        public static Term ConvertEnumerableAnyToTerm(MethodCallExpression methodCall, DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var target = methodCall.Arguments[0];
            var predicate = methodCall.Arguments[1];

            var filterTerm = new Term()
            {
                type = Term.TermType.CONTAINS
            };
            filterTerm.args.Add(recursiveMap(target));

            var enumerableElementType = methodCall.Method.GetGenericArguments()[0];
            var createFunctionTermMethod = typeof(ExpressionUtils)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "CreateFunctionTerm" && m.GetGenericArguments().Length == 2);
            createFunctionTermMethod = createFunctionTermMethod.MakeGenericMethod(enumerableElementType, typeof(bool));

            var functionTerm = (Term)createFunctionTermMethod.Invoke(null, new object[] { new QueryConverter(datumConverterFactory, expressionConverterFactory), predicate });
            filterTerm.args.Add(functionTerm);

            return filterTerm;
        }
    }
}
