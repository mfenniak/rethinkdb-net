using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    static class ExpressionUtils
    {
        private static Term ConvertBinaryExpressionToTerm(Func<Expression, Term> recursiveMap, BinaryExpression expr, Term.TermType termType)
        {
            var term = new Term() {
                type = termType
            };
            term.args.Add(recursiveMap(expr.Left));
            term.args.Add(recursiveMap(expr.Right));
            return term;
        }

        private static Term ConvertUnaryExpressionToTerm(Func<Expression, Term> recursiveMap, UnaryExpression expr, Term.TermType termType)
        {
            var term = new Term() {
                type = termType
            };
            term.args.Add(recursiveMap(expr.Operand));
            return term;
        }

        private static Term SimpleMap(IDatumConverterFactory datumConverterFactory, Func<Expression, Term> recursiveMap, Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                {
                    var constantExpression = (ConstantExpression)expr;

                    // FIXME: could avoid reflection here easily if there were non-generic methods on IDatumConverterFactory and IDatumConverter
                    var conversionMethod = typeof(ExpressionUtils).GetMethod("ReflectedConstantConversion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    conversionMethod = conversionMethod.MakeGenericMethod(new Type[] { constantExpression.Type });

                    var datum = (Datum)conversionMethod.Invoke(null, new object[] { datumConverterFactory, constantExpression.Value });
                    return new Term() {
                        type = Term.TermType.DATUM,
                        datum = datum
                    };
                }

                case ExpressionType.Add:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.ADD);
                case ExpressionType.Modulo:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.MOD);
                case ExpressionType.Divide:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.DIV);
                case ExpressionType.Multiply:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.MUL);
                case ExpressionType.Subtract:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.SUB);
                case ExpressionType.Equal:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.EQ);
                case ExpressionType.LessThan:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.LT);
                case ExpressionType.LessThanOrEqual:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.LE);
                case ExpressionType.GreaterThan:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.GT);
                case ExpressionType.GreaterThanOrEqual:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.GE);
                case ExpressionType.AndAlso:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.ALL);
                case ExpressionType.OrElse:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.ANY);
                case ExpressionType.NotEqual:
                    return ConvertBinaryExpressionToTerm(recursiveMap, (BinaryExpression)expr, Term.TermType.NE);
                case ExpressionType.Not:
                    return ConvertUnaryExpressionToTerm(recursiveMap, (UnaryExpression)expr, Term.TermType.NOT);

                default:
                    throw new NotSupportedException(String.Format("Unsupported expression type: {0}", expr.NodeType));
            }
        }

        public static Term MapLambdaToFunction<TParameter1, TParameter2>(IDatumConverterFactory datumConverterFactory, LambdaExpression expr)
        {
            var funcTerm = new Term() {
                type = Term.TermType.FUNC
            };

            var parametersTerm = new Term() {
                type = Term.TermType.MAKE_ARRAY,
            };
            parametersTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = 1
                }
            });
            parametersTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = 2
                }
            });
            funcTerm.args.Add(parametersTerm);
            funcTerm.args.Add(ExpressionUtils.MapExpressionToTerm<TParameter1, TParameter2>(datumConverterFactory, expr.Body, expr.Parameters[0].Name, expr.Parameters[1].Name));
            return funcTerm;
        }

        public static Term MapExpressionToTerm<TParameter1, TParameter2>(IDatumConverterFactory datumConverterFactory, Expression expr, string parameter1Name, string parameter2Name)
        {
            Func<Expression, Term> recursiveMap = (innerExpr) => MapExpressionToTerm<TParameter1, TParameter2>(datumConverterFactory, innerExpr, parameter1Name, parameter2Name);

            switch (expr.NodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    var memberExpr = (MemberExpression)expr;

                    if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                        throw new NotSupportedException("Unrecognized member access pattern");

                    var parameterExpr = (ParameterExpression)memberExpr.Expression;
                    int parameterIndex;
                    if (parameterExpr.Name == parameter1Name)
                        parameterIndex = 1;
                    else if (parameterExpr.Name == parameter2Name)
                        parameterIndex = 2;
                    else
                        throw new InvalidOperationException("Unmatched parameter name:" + parameterExpr.Name);

                    var getAttrTerm = new Term() {
                        type = Term.TermType.GETATTR
                    };

                    getAttrTerm.args.Add(new Term() {
                        type = Term.TermType.VAR,
                        args = {
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_NUM,
                                    r_num = parameterIndex
                                },
                            }
                        }
                    });

                    if (parameterIndex == 1)
                    {
                        var datumConverter = datumConverterFactory.Get<TParameter1>();
                        var fieldConverter = datumConverter as IObjectDatumConverter;
                        if (fieldConverter == null)
                            throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

                        getAttrTerm.args.Add(new Term() {
                            type = Term.TermType.DATUM,
                            datum = new Datum() {
                                type = Datum.DatumType.R_STR,
                                r_str = fieldConverter.GetDatumFieldName(memberExpr.Member)
                            }
                        });
                    }
                    else if (parameterIndex == 2)
                    {
                        var datumConverter = datumConverterFactory.Get<TParameter2>();
                        var fieldConverter = datumConverter as IObjectDatumConverter;
                        if (fieldConverter == null)
                            throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

                        getAttrTerm.args.Add(new Term() {
                            type = Term.TermType.DATUM,
                            datum = new Datum() {
                                type = Datum.DatumType.R_STR,
                                r_str = fieldConverter.GetDatumFieldName(memberExpr.Member)
                            }
                        });
                    }

                    return getAttrTerm;
                }

                default:
                    return SimpleMap(datumConverterFactory, recursiveMap, expr);
            }
        }

        public static Term MapExpressionToTerm<TParameter1>(IDatumConverterFactory datumConverterFactory, Expression expr)
        {
            Func<Expression, Term> recursiveMap = (innerExpr) => MapExpressionToTerm<TParameter1>(datumConverterFactory, innerExpr);

            switch (expr.NodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    var memberExpr = (MemberExpression)expr;

                    if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                        throw new NotSupportedException("Unrecognized member access pattern");

                    var getAttrTerm = new Term() {
                        type = Term.TermType.GETATTR
                    };

                    getAttrTerm.args.Add(new Term() {
                        type = Term.TermType.VAR,
                        args = {
                            new Term() {
                                type = Term.TermType.DATUM,
                                datum = new Datum() {
                                    type = Datum.DatumType.R_NUM,
                                    r_num = 2
                                },
                            }
                        }
                    });

                    var datumConverter = datumConverterFactory.Get<TParameter1>();
                    var fieldConverter = datumConverter as IObjectDatumConverter;
                    if (fieldConverter == null)
                        throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

                    getAttrTerm.args.Add(new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_STR,
                            r_str = fieldConverter.GetDatumFieldName(memberExpr.Member)
                        }
                    });

                    return getAttrTerm;
                }

                default:
                    return SimpleMap(datumConverterFactory, recursiveMap, expr);
            }
        }

        private static Datum ReflectedConstantConversion<TInnerType>(IDatumConverterFactory datumFactory, TInnerType obj)
        {
            var converter = datumFactory.Get<TInnerType>();
            return converter.ConvertObject(obj);
        }
    }
}

