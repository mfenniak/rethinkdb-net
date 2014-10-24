using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    abstract class BaseExpression
    {
        #region Constructor

        protected readonly DefaultExpressionConverterFactory expressionConverterFactory;

        protected BaseExpression(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            this.expressionConverterFactory = expressionConverterFactory;
        }

        #endregion
        #region Parameter-independent Mappings

        protected abstract Term RecursiveMap(Expression expression);

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

        private Term ConvertDateTimeAddFunctionToTerm(MethodCallExpression callExpression, double conversionToSeconds)
        {
            return new Term() {
                type = Term.TermType.ADD,
                args = {
                    RecursiveMap(callExpression.Object),
                    new Term() {
                        type = Term.TermType.MUL,
                        args = {
                            RecursiveMap(callExpression.Arguments[0]),
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_NUM,
                                    r_num = conversionToSeconds
                                }
                            }
                        }
                    }
                }
            };
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
                        return AttemptClientSideConversion(datumConverterFactory, expr);

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

                    DefaultExpressionConverterFactory.MethodCallMappingDelegate methodCallMapping;
                    if (expressionConverterFactory.TryGetMethodCallMapping(method, out methodCallMapping))
                        return methodCallMapping(callExpression, RecursiveMap, datumConverterFactory, expressionConverterFactory);
                    else
                        return AttemptClientSideConversion(datumConverterFactory, expr);
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

