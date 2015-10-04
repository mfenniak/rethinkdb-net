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
        private static IDictionary<ExpressionType, Term.TermType> DefaultTermTypes = new Dictionary<ExpressionType, Term.TermType>
        {
            { ExpressionType.Add, Term.TermType.ADD },
            { ExpressionType.Modulo, Term.TermType.MOD },
            { ExpressionType.Divide, Term.TermType.DIV },
            { ExpressionType.Multiply, Term.TermType.MUL },
            { ExpressionType.Subtract, Term.TermType.SUB },
            { ExpressionType.Equal, Term.TermType.EQ },
            { ExpressionType.LessThan, Term.TermType.LT },
            { ExpressionType.LessThanOrEqual, Term.TermType.LE },
            { ExpressionType.GreaterThan, Term.TermType.GT },
            { ExpressionType.GreaterThanOrEqual, Term.TermType.GE },
            { ExpressionType.AndAlso, Term.TermType.AND },
            { ExpressionType.OrElse, Term.TermType.OR },
            { ExpressionType.NotEqual, Term.TermType.NE },
            { ExpressionType.Not, Term.TermType.NOT },
            { ExpressionType.ArrayLength, Term.TermType.COUNT },
            { ExpressionType.ArrayIndex, Term.TermType.NTH },
        };

        #region Constructor

        protected readonly DefaultExpressionConverterFactory expressionConverterFactory;

        protected BaseExpression(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            this.expressionConverterFactory = expressionConverterFactory;
        }

        #endregion
        #region Parameter-independent Mappings

        protected abstract Term RecursiveMap(Expression expression, bool allowMemberInit = false);

        private Term ConvertBinaryExpressionToTerm(BinaryExpression expr, IDatumConverterFactory datumConverterFactory)
        {
            DefaultExpressionConverterFactory.ExpressionMappingDelegate<BinaryExpression> binaryExpressionMapping;
            Term.TermType defaultTermType;

            if (expressionConverterFactory.TryGetBinaryExpressionMapping(expr.Left.Type, expr.Right.Type, expr.NodeType, out binaryExpressionMapping))
            {
                return binaryExpressionMapping(expr, RecursiveMap, datumConverterFactory, expressionConverterFactory);
            }
            else if (DefaultTermTypes.TryGetValue(expr.NodeType, out defaultTermType))
            {
                var term = new Term() {
                    type = defaultTermType
                };
                term.args.Add(RecursiveMap(expr.Left));
                term.args.Add(RecursiveMap(expr.Right));
                return term;
            }
            else
                return AttemptClientSideConversion(datumConverterFactory, expr);
        }

        private Term ConvertUnaryExpressionToTerm(UnaryExpression expr, IDatumConverterFactory datumConverterFactory)
        {
            DefaultExpressionConverterFactory.ExpressionMappingDelegate<UnaryExpression> unaryExpressionMapping;
            Term.TermType defaultTermType;

            if (expressionConverterFactory.TryGetUnaryExpressionMapping(expr.Operand.Type, expr.NodeType, out unaryExpressionMapping))
            {
                return unaryExpressionMapping(expr, RecursiveMap, datumConverterFactory, expressionConverterFactory);
            }
            else if (DefaultTermTypes.TryGetValue(expr.NodeType, out defaultTermType))
            {
                var term = new Term() {
                    type = defaultTermType
                };
                term.args.Add(RecursiveMap(expr.Operand));
                return term;
            }
            else
                return AttemptClientSideConversion(datumConverterFactory, expr);
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
                case ExpressionType.Modulo:
                case ExpressionType.Divide:
                case ExpressionType.Multiply:
                case ExpressionType.Subtract:
                case ExpressionType.Equal:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.NotEqual:
                case ExpressionType.ArrayIndex:
                    return ConvertBinaryExpressionToTerm((BinaryExpression)expr, datumConverterFactory);
                case ExpressionType.Not:
                case ExpressionType.ArrayLength:
                    return ConvertUnaryExpressionToTerm((UnaryExpression)expr, datumConverterFactory);

                case ExpressionType.New:
                {
                    var newExpression = (NewExpression)expr;
                    if (AnonymousTypeDatumConverterFactory.Instance.IsTypeSupported(newExpression.Type))
                    {
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

                    DefaultExpressionConverterFactory.ExpressionMappingDelegate<NewExpression> newExpressionMapping;
                    if (expressionConverterFactory.TryGetNewExpressionMapping(newExpression.Constructor, out newExpressionMapping))
                        return newExpressionMapping(newExpression, RecursiveMap, datumConverterFactory, expressionConverterFactory);

                    return AttemptClientSideConversion(datumConverterFactory, expr);
                }

                case ExpressionType.NewArrayInit:
                    var arrayExpression = (NewArrayExpression)expr;
                    var array = new Term {type = Term.TermType.MAKE_ARRAY};

                    foreach (var expression in arrayExpression.Expressions)
                    {
                        array.args.Add(RecursiveMap(expression));
                    }

                    return array;

                case ExpressionType.Call:
                {
                    var callExpression = (MethodCallExpression)expr;
                    var method = callExpression.Method;

                    DefaultExpressionConverterFactory.ExpressionMappingDelegate<MethodCallExpression> methodCallMapping;
                    if (expressionConverterFactory.TryGetMethodCallMapping(method, out methodCallMapping))
                        return methodCallMapping(callExpression, RecursiveMap, datumConverterFactory, expressionConverterFactory);
                    else
                        return AttemptClientSideConversion(datumConverterFactory, expr);
                }

                case ExpressionType.MemberAccess:
                {
                    var memberExpression = (MemberExpression)expr;
                    var member = memberExpression.Member;

                    DefaultExpressionConverterFactory.ExpressionMappingDelegate<MemberExpression> memberAccessMapping;
                    if (expressionConverterFactory.TryGetMemberAccessMapping(member, out memberAccessMapping))
                        return memberAccessMapping(memberExpression, RecursiveMap, datumConverterFactory, expressionConverterFactory);

                    Term serverSideTerm;
                    if (ServerSideMemberAccess(datumConverterFactory, memberExpression, out serverSideTerm))
                        return serverSideTerm;

                    return AttemptClientSideConversion(datumConverterFactory, expr);
                }

                case ExpressionType.Conditional:
                {
                    var conditionalExpression = (ConditionalExpression)expr;
                    return new Term()
                    {
                        type = Term.TermType.BRANCH,
                        args = {
                            RecursiveMap(conditionalExpression.Test),
                            RecursiveMap(conditionalExpression.IfTrue, true),
                            RecursiveMap(conditionalExpression.IfFalse, true)
                        }
                    };
                }

                case ExpressionType.Convert:
                {
                    // The use-case for this right now is automatic boxing that occurs when setting a Dictionary<string,object>'s value
                    // to a primitive.  In that particular case, we don't actually need to generate any ReQL for the conversion, so we're
                    // just ignoring the Convert and mapping the expression inside.  Might need other behavior here in the future...
                    return RecursiveMap(((UnaryExpression)expr).Operand);
                }

                default:
                {
                    return AttemptClientSideConversion(datumConverterFactory, expr);
                }
            }
        }

        private bool ServerSideMemberAccess(IDatumConverterFactory datumConverterFactory, MemberExpression memberExpression, out Term term)
        {
            term = null;

            if (memberExpression.Expression == null)
                // static member access; can't do that server-side, wouldn't want to either.
                return false;

            if (memberExpression.Expression.NodeType == ExpressionType.Constant)
                // Accessing a constant; client-side conversion will be more efficient since we won't be round-tripping the object
                return false;
            
            IDatumConverter datumConverter;
            if (!datumConverterFactory.TryGet(memberExpression.Expression.Type, out datumConverter))
                // No datum converter for this type.
                return false;

            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                // doesn't implement IObjectDatumConverter, so we don't know how to map the MemberInfo to the field name
                return false;

            var datumFieldName = fieldConverter.GetDatumFieldName(memberExpression.Member);
            if (string.IsNullOrEmpty(datumFieldName))
            {
                // At this point we're not returning false because we're expecting this should work; throwing an error instead.
                // We're expecting this to work, but, returning false will give us a chance to try client-side evaluation instead and
                // maintain compatibility with the pre-PR #209 behaviour. (issue #220).
                return false;
            }

            var getAttrTerm = new Term()
            {
                type = Term.TermType.GET_FIELD
            };
            getAttrTerm.args.Add(RecursiveMap(memberExpression.Expression));
            getAttrTerm.args.Add(new Term()
            {
                type = Term.TermType.DATUM,
                datum = new Datum()
                {
                    type = Datum.DatumType.R_STR,
                    r_str = datumFieldName
                }
            });

            term = getAttrTerm;
            return true;
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
                throw new InvalidOperationException(
                    String.Format(
                        "Failed to perform client-side evaluation of expression tree node '{0}'; this is caused by refering to a server-side variable in an expression tree that isn't convertible to ReQL logic",
                        expr),
                    ex);
            }
        }

        #endregion
    }
}

