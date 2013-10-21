using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RethinkDb.DatumConverters;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    abstract class BaseExpression
    {
        private static Lazy<MethodInfo> ReQLAppend = new Lazy<MethodInfo>(() =>
            typeof(ReQLExpression).GetMethod("Append", BindingFlags.Static | BindingFlags.Public)
        );

        private static Lazy<MethodInfo> EnumerableWhereWithSimplePredicate = new Lazy<MethodInfo>(() =>
            typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "Where" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))
        );

        private static Lazy<MethodInfo> EnumerableCountWithNoPredicate = new Lazy<MethodInfo>(() =>
            typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "Count" && m.GetParameters().Length == 1)
        );

        #region Parameter-independent Mappings

        protected abstract Term RecursiveMap(Expression expression);
        protected abstract Term RecursiveMapMemberInit<TInnerReturn>(Expression expression);

        private Term ConvertBinaryExpressionToTerm(BinaryExpression expr, Term.TermType termType)
        {
            var term = new Term() {
                type = termType
            };
            term.args.Add(RecursiveMap(expr.Left));
            term.args.Add(RecursiveMap(expr.Right));
            return term;
        }

        private Term ConvertUnaryExpressionToTerm(UnaryExpression expr, Term.TermType termType)
        {
            var term = new Term() {
                type = termType
            };
            term.args.Add(RecursiveMap(expr.Operand));
            return term;
        }

        protected Term SimpleMap(IDatumConverterFactory datumConverterFactory, Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                {
                    var constantExpression = (ConstantExpression)expr;
                    var datumConverter = datumConverterFactory.Get(constantExpression.Type);
                    var datum = datumConverter.ConvertObject(constantExpression.Value);
                    return new Term() {
                        type = Term.TermType.DATUM,
                        datum = datum
                    };
                }

                case ExpressionType.Add:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.ADD);
                case ExpressionType.Modulo:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.MOD);
                case ExpressionType.Divide:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.DIV);
                case ExpressionType.Multiply:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.MUL);
                case ExpressionType.Subtract:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.SUB);
                case ExpressionType.Equal:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.EQ);
                case ExpressionType.LessThan:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.LT);
                case ExpressionType.LessThanOrEqual:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.LE);
                case ExpressionType.GreaterThan:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.GT);
                case ExpressionType.GreaterThanOrEqual:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.GE);
                case ExpressionType.AndAlso:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.ALL);
                case ExpressionType.OrElse:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.ANY);
                case ExpressionType.NotEqual:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, Term.TermType.NE);
                case ExpressionType.Not:
                    return ConvertUnaryExpressionToTerm((UnaryExpression)expr, Term.TermType.NOT);
                case ExpressionType.ArrayLength:
                    return ConvertUnaryExpressionToTerm((UnaryExpression)expr, Term.TermType.COUNT);

                case ExpressionType.New:
                {
                    var newExpression = (NewExpression)expr;
                    if (!AnonymousTypeDatumConverterFactory.Instance.IsTypeSupported(newExpression.Type))
                        throw new NotSupportedException(String.Format("Unsupported type in New expression: {0}; only anonymous types are supported", expr.Type));

                    var retval = new Term() {
                        type = Term.TermType.MAKE_OBJ,
                    };
                    foreach (var property in newExpression.Type.GetProperties().Select((p, i) => new { Property = p, Index = i }))
                    {
                        var key = property.Property.Name;
                        var value = RecursiveMap(newExpression.Arguments[property.Index]);
                        retval.optargs.Add(new Term.AssocPair() {
                            key = key, val = value
                        });
                    }
                    return retval;
                }

                case ExpressionType.Call:
                {
                    var callExpression = (MethodCallExpression)expr;
                    var method = callExpression.Method;

                    if (method.IsGenericMethod && method.GetGenericMethodDefinition() == ReQLAppend.Value)
                    {
                        var target = callExpression.Arguments[0];

                        var appendArray = callExpression.Arguments[1];
                        if (appendArray.NodeType != ExpressionType.NewArrayInit)
                            throw new NotSupportedException(String.Format("Expected second arg to ReQLExpression.Append to be NewArrayInit, but was: {0}", appendArray.NodeType));

                        var newArrayExpression = (NewArrayExpression)appendArray;
                        var term = RecursiveMap(target);
                        foreach (var datumExpression in newArrayExpression.Expressions)
                        {
                            var newTerm = new Term() {
                                type = Term.TermType.APPEND
                            };
                            newTerm.args.Add(term);

                            if (datumExpression.NodeType == ExpressionType.MemberInit)
                            {
                                var memberInit = (MemberInitExpression)datumExpression;

                                var recursiveMapMethod = typeof(BaseExpression).GetMethod("RecursiveMapMemberInit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                recursiveMapMethod = recursiveMapMethod.MakeGenericMethod(new Type[] { memberInit.Type });
                                newTerm.args.Add((Term)recursiveMapMethod.Invoke(this, new object[] { memberInit }));
                            }
                            else
                                throw new NotSupportedException(String.Format("Expected ReQLExpression.Append to contain MemberInit additions, but was: {0}", datumExpression.NodeType));

                            term = newTerm;
                        }

                        return term;
                    }
                    else if (method.IsGenericMethod && method.GetGenericMethodDefinition() == EnumerableWhereWithSimplePredicate.Value)
                    {
                        var target = callExpression.Arguments[0];
                        var predicate = callExpression.Arguments[1];

                        var filterTerm = new Term()
                        {
                            type = Term.TermType.FILTER
                        };
                        
                        filterTerm.args.Add(RecursiveMap(target));

                        var enumerableElementType = method.ReturnType.GetGenericArguments()[0];
                        var createFunctionTermMethod = typeof(ExpressionUtils)
                            .GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Single(m => m.Name == "CreateFunctionTerm" && 
                                         m.GetGenericArguments().Length == 2);
                        createFunctionTermMethod = createFunctionTermMethod.MakeGenericMethod(enumerableElementType, typeof(bool));

                        var functionTerm = (Term)createFunctionTermMethod.Invoke(null, new object[] { datumConverterFactory, predicate });
                        filterTerm.args.Add(functionTerm);

                        return filterTerm;
                    }
                    else if (method.IsGenericMethod && method.GetGenericMethodDefinition() == EnumerableCountWithNoPredicate.Value)
                    {
                        var target = callExpression.Arguments[0];

                        var countTerm = new Term()
                        {
                            type = Term.TermType.COUNT,
                        };
                        countTerm.args.Add(RecursiveMap(target));
                        return countTerm;
                    }
                    else
                    {
                        return AttemptClientSideConversion(datumConverterFactory, expr);
                    }
                }

                case ExpressionType.MemberAccess:
                {
                    var memberExpression = (MemberExpression)expr;
                    var member = memberExpression.Member;

                    if (member.DeclaringType.IsGenericType &&
                        (
                            member.DeclaringType.GetGenericTypeDefinition() == typeof(List<>) ||
                            member.DeclaringType.GetGenericTypeDefinition() == typeof(ICollection<>)
                        ) &&
                        member.Name == "Count")
                    {
                        var countTerm = new Term()
                        {
                            type = Term.TermType.COUNT,
                        };
                        countTerm.args.Add(RecursiveMap(memberExpression.Expression));
                        return countTerm;
                    }
                    else
                    {
                        return AttemptClientSideConversion(datumConverterFactory, expr);
                    }
                }

                default:
                {
                    return AttemptClientSideConversion(datumConverterFactory, expr);
                }
            }
        }

        private Term AttemptClientSideConversion(IDatumConverterFactory datumConverterFactory, Expression expr)
        {
            try
            {
                var converter = datumConverterFactory.Get(expr.Type);
                var clientSideFunc = Expression.Lambda(expr).Compile();
                return new Term() {
                    type = Term.TermType.DATUM,
                    datum = converter.ConvertObject(clientSideFunc.DynamicInvoke())
                };
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Failed to perform client-side evaluation of expression tree node; often this is caused by refering to a server-side variable in a node that is only supported w/ client-side evaluation", ex);
            }
        }

        #endregion
    }
}

