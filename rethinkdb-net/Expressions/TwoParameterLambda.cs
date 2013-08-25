using RethinkDb.Spec;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RethinkDb.Expressions
{
    // Note: Not thread-safe, do not share an instance across threads.
    class TwoParameterLambda<TParameter1, TParameter2, TReturn> : BaseExpression
    {
        #region Public interface

        private readonly IDatumConverterFactory datumConverterFactory;
        private string parameter1Name;
        private string parameter2Name;

        public TwoParameterLambda(IDatumConverterFactory datumConverterFactory)
        {
            this.datumConverterFactory = datumConverterFactory;
        }

        public Term CreateFunctionTerm(Expression<Func<TParameter1, TParameter2, TReturn>> expression)
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

            this.parameter1Name = expression.Parameters[0].Name;
            this.parameter2Name = expression.Parameters[1].Name;

            var body = expression.Body;
            if (body.NodeType == ExpressionType.MemberInit)
            {
                var memberInit = (MemberInitExpression)body;
                if (!memberInit.Type.Equals(typeof(TReturn)))
                    throw new InvalidOperationException("Only expression types matching the table type are supported");
                else if (memberInit.NewExpression.Arguments.Count != 0)
                    throw new NotSupportedException("Constructors will not work here, only field member initialization");
                funcTerm.args.Add(MapMemberInitToTerm(memberInit));
            }
            else
            {
                funcTerm.args.Add(MapExpressionToTerm(expression.Body));
            }

            return funcTerm;
        }

        #endregion
        #region Abstract implementation

        private Term MapMemberInitToTerm(MemberInitExpression memberInit)
        {
            var makeObjTerm = new Term() {
                type = Term.TermType.MAKE_OBJ,
            };

            foreach (var binding in memberInit.Bindings)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        makeObjTerm.optargs.Add(MapMemberAssignmentToMakeObjArg((MemberAssignment)binding));
                        break;
                    case MemberBindingType.ListBinding:
                    case MemberBindingType.MemberBinding:
                        throw new NotSupportedException("Binding type not currently supported");
                }
            }

            return makeObjTerm;
        }

        private Term.AssocPair MapMemberAssignmentToMakeObjArg(MemberAssignment memberAssignment)
        {
            var retval = new Term.AssocPair();

            var datumConverter = datumConverterFactory.Get<TReturn>();
            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member assignments into ReQL without implementing IObjectDatumConverter");

            retval.key = fieldConverter.GetDatumFieldName(memberAssignment.Member);
            retval.val = MapExpressionToTerm(memberAssignment.Expression);

            return retval;
        }

        private Term MapExpressionToTerm(Expression expr)
        {
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
                        type = Term.TermType.GET_FIELD
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
                    return SimpleMap(datumConverterFactory, expr);
            }
        }

        protected override Term RecursiveMap(Expression expression)
        {
            return MapExpressionToTerm(expression);
        }

        protected override Term RecursiveMapMemberInit<TInnerReturn>(Expression expression)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}

