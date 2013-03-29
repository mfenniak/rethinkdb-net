using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    static class ExpressionUtils
    {
        #region Public interface 

        public static Term CreateValueTerm<TReturn>(IDatumConverterFactory datumConverterFactory, Expression<Func<TReturn>> expression)
        {
            Func<Expression, Term> recursiveMap = (expr) => SimpleMap(datumConverterFactory, recursiveMap, expr);
            return SimpleMap(datumConverterFactory, recursiveMap, expression.Body);
        }

        public static Term CreateFunctionTerm<TParameter1, TReturn>(IDatumConverterFactory datumConverterFactory, Expression<Func<TParameter1, TReturn>> expression)
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
                    r_num = 2
                }
            });
            funcTerm.args.Add(parametersTerm);

            var body = expression.Body;
            if (body.NodeType == ExpressionType.MemberInit)
            {
                var memberInit = (MemberInitExpression)body;
                if (!memberInit.Type.Equals(typeof(TReturn)))
                    throw new InvalidOperationException("Only expression types matching the table type are supported");
                else if (memberInit.NewExpression.Arguments.Count != 0)
                    throw new NotSupportedException("Constructors will not work here, only field member initialization");
                funcTerm.args.Add(MapMemberInitToTerm<TParameter1, TReturn>(datumConverterFactory, memberInit));
            }
            else
            {
                funcTerm.args.Add(ExpressionUtils.MapExpressionToTerm<TParameter1>(datumConverterFactory, expression.Body));
            }

            return funcTerm;
        }

        public static Term CreateFunctionTerm<TParameter1, TParameter2, TReturn>(IDatumConverterFactory datumConverterFactory, Expression<Func<TParameter1, TParameter2, TReturn>> expression)
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
                    r_num = 3
                }
            });
            parametersTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_NUM,
                    r_num = 4
                }
            });
            funcTerm.args.Add(parametersTerm);

            var body = expression.Body;
            if (body.NodeType == ExpressionType.MemberInit)
            {
                var memberInit = (MemberInitExpression)body;
                if (!memberInit.Type.Equals(typeof(TReturn)))
                    throw new InvalidOperationException("Only expression types matching the table type are supported");
                else if (memberInit.NewExpression.Arguments.Count != 0)
                    throw new NotSupportedException("Constructors will not work here, only field member initialization");
                funcTerm.args.Add(MapMemberInitToTerm<TParameter1, TParameter2, TReturn>(datumConverterFactory, memberInit, expression.Parameters[0].Name, expression.Parameters[1].Name));
            }
            else
            {
                funcTerm.args.Add(ExpressionUtils.MapExpressionToTerm<TParameter1, TParameter2>(datumConverterFactory, expression.Body, expression.Parameters[0].Name, expression.Parameters[1].Name));
            }

            return funcTerm;
        }

        #endregion
        #region Single Parameter Mappings

        private static Term MapMemberInitToTerm<TParameter1, TReturn>(IDatumConverterFactory datumConverterFactory, MemberInitExpression memberInit)
        {
            var makeObjTerm = new Term() {
                type = Term.TermType.MAKE_OBJ,
            };

            foreach (var binding in memberInit.Bindings)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        makeObjTerm.optargs.Add(MapMemberAssignmentToMakeObjArg<TParameter1, TReturn>(datumConverterFactory, (MemberAssignment)binding));
                        break;
                    case MemberBindingType.ListBinding:
                    case MemberBindingType.MemberBinding:
                        throw new NotSupportedException("Binding type not currently supported");
                }
            }

            return makeObjTerm;
        }

        private static Term.AssocPair MapMemberAssignmentToMakeObjArg<TParameter1, TReturn>(IDatumConverterFactory datumConverterFactory, MemberAssignment memberAssignment)
        {
            var retval = new Term.AssocPair();

            var datumConverter = datumConverterFactory.Get<TReturn>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member assignments into ReQL without implementing IObjectDatumConverter");

            retval.key = fieldConverter.GetDatumFieldName(memberAssignment.Member);
            retval.val = ExpressionUtils.MapExpressionToTerm<TParameter1>(datumConverterFactory, memberAssignment.Expression);

            return retval;
        }

        private static Term MapExpressionToTerm<TParameter1>(IDatumConverterFactory datumConverterFactory, Expression expr)
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

                    var datumFieldName = fieldConverter.GetDatumFieldName(memberExpr.Member);
                    if (string.IsNullOrEmpty(datumFieldName))
                        throw new NotSupportedException(String.Format("Member {0} on type {1} could not be mapped to a datum field", memberExpr.Member.Name, memberExpr.Type));

                    getAttrTerm.args.Add(new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_STR,
                            r_str = datumFieldName
                        }
                    });

                    return getAttrTerm;
                }

                default:
                    return SimpleMap(datumConverterFactory, recursiveMap, expr);
            }
        }

        #endregion
        #region Two Parameter Mappings

        private static Term MapMemberInitToTerm<TParameter1, TParameter2, TReturn>(IDatumConverterFactory datumConverterFactory, MemberInitExpression memberInit, string parameter1Name, string parameter2Name)
        {
            var makeObjTerm = new Term() {
                type = Term.TermType.MAKE_OBJ,
            };

            foreach (var binding in memberInit.Bindings)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        makeObjTerm.optargs.Add(MapMemberAssignmentToMakeObjArg<TParameter1, TParameter2, TReturn>(datumConverterFactory, (MemberAssignment)binding, parameter1Name, parameter2Name));
                        break;
                    case MemberBindingType.ListBinding:
                    case MemberBindingType.MemberBinding:
                        throw new NotSupportedException("Binding type not currently supported");
                }
            }

            return makeObjTerm;
        }

        private static Term.AssocPair MapMemberAssignmentToMakeObjArg<TParameter1, TParameter2, TReturn>(IDatumConverterFactory datumConverterFactory, MemberAssignment memberAssignment, string parameter1Name, string parameter2Name)
        {
            var retval = new Term.AssocPair();

            var datumConverter = datumConverterFactory.Get<TReturn>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member assignments into ReQL without implementing IObjectDatumConverter");

            retval.key = fieldConverter.GetDatumFieldName(memberAssignment.Member);
            retval.val = ExpressionUtils.MapExpressionToTerm<TParameter1, TParameter2>(datumConverterFactory, memberAssignment.Expression, parameter1Name, parameter2Name);

            return retval;
        }

        private static Term MapExpressionToTerm<TParameter1, TParameter2>(IDatumConverterFactory datumConverterFactory, Expression expr, string parameter1Name, string parameter2Name)
        {
            Func<Expression, Term> recursiveMap = (innerExpr) => MapExpressionToTerm<TParameter1, TParameter2>(datumConverterFactory, innerExpr, parameter1Name, parameter2Name);

            switch (expr.NodeType)
            {
                case ExpressionType.Parameter:
                {
                    var parameterExpr = (ParameterExpression)expr;
                    int parameterIndex;
                    if (parameterExpr.Name == parameter1Name)
                        parameterIndex = 3;
                    else if (parameterExpr.Name == parameter2Name)
                        parameterIndex = 4;
                    else
                        throw new InvalidOperationException("Unmatched parameter name:" + parameterExpr.Name);

                    return new Term() {
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
                    };
                }

                case ExpressionType.MemberAccess:
                {
                    var memberExpr = (MemberExpression)expr;

                    if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                        throw new NotSupportedException("Unrecognized member access pattern");

                    var parameterExpr = (ParameterExpression)memberExpr.Expression;
                    int parameterIndex;
                    if (parameterExpr.Name == parameter1Name)
                        parameterIndex = 3;
                    else if (parameterExpr.Name == parameter2Name)
                        parameterIndex = 4;
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

                    if (parameterIndex == 3)
                    {
                        var datumConverter = datumConverterFactory.Get<TParameter1>();
                        var fieldConverter = datumConverter as IObjectDatumConverter;
                        if (fieldConverter == null)
                            throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

                        var datumFieldName = fieldConverter.GetDatumFieldName(memberExpr.Member);
                        if (string.IsNullOrEmpty(datumFieldName))
                            throw new NotSupportedException(String.Format("Member {0} on type {1} could not be mapped to a datum field", memberExpr.Member.Name, memberExpr.Type));

                        getAttrTerm.args.Add(new Term() {
                            type = Term.TermType.DATUM,
                            datum = new Datum() {
                                type = Datum.DatumType.R_STR,
                                r_str = datumFieldName
                            }
                        });
                    }
                    else if (parameterIndex == 4)
                    {
                        var datumConverter = datumConverterFactory.Get<TParameter2>();
                        var fieldConverter = datumConverter as IObjectDatumConverter;
                        if (fieldConverter == null)
                            throw new NotSupportedException("Cannot map member access into ReQL without implementing IObjectDatumConverter");

                        var datumFieldName = fieldConverter.GetDatumFieldName(memberExpr.Member);
                        if (string.IsNullOrEmpty(datumFieldName))
                            throw new NotSupportedException(String.Format("Member {0} on type {1} could not be mapped to a datum field", memberExpr.Member.Name, memberExpr.Type));

                        getAttrTerm.args.Add(new Term() {
                            type = Term.TermType.DATUM,
                            datum = new Datum() {
                                type = Datum.DatumType.R_STR,
                                r_str = datumFieldName
                            }
                        });
                    }

                    return getAttrTerm;
                }

                default:
                    return SimpleMap(datumConverterFactory, recursiveMap, expr);
            }
        }

        #endregion
        #region Parameter-independent Mappings

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

        private static Datum ReflectedConstantConversion<TInnerType>(IDatumConverterFactory datumFactory, TInnerType obj)
        {
            var converter = datumFactory.Get<TInnerType>();
            return converter.ConvertObject(obj);
        }

        #endregion
    }
}

