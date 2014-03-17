using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    class SingleParameterLambda<TParameter1, TReturn> : BaseExpression
    {
        #region Public interface

        private readonly IDatumConverterFactory datumConverterFactory;

        public SingleParameterLambda(IDatumConverterFactory datumConverterFactory)
        {
            this.datumConverterFactory = datumConverterFactory;
        }

        public Term CreateFunctionTerm(Expression<Func<TParameter1, TReturn>> expression)
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
                funcTerm.args.Add(MapMemberInitToTerm(memberInit));
            }
            else
            {
                funcTerm.args.Add(MapExpressionToTerm(expression.Body));
            }

            return funcTerm;
        }

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

        protected override Term RecursiveMap(Expression expression)
        {
            return MapExpressionToTerm(expression);
        }

        protected override Term RecursiveMapMemberInit<TInnerReturn>(Expression expression)
        {
            var newConverter = new SingleParameterLambda<TParameter1, TInnerReturn>(datumConverterFactory);
            return newConverter.MapMemberInitToTerm((MemberInitExpression)expression);
        }

        private Term MapExpressionToTerm(Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Parameter:
                {
                    return new Term()
                    {
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
                    };
                }

                case ExpressionType.MemberAccess:
                {
                    var memberExpr = (MemberExpression)expr;

                    if ((memberExpr.Expression == null || memberExpr.Expression.NodeType != ExpressionType.Parameter) &&
                        // FIXME: Issue #169: In some cases the CLR can insert a type-cast when a generic type constrant is present on a generic type that's a parameter.
                        // This is a really hacky work-around: if there's a cast on the expression, we and the expression inside is a parameter, we just proceed like it's
                        // a normal member access and ignore the cast.  Need to investigate a better fix.
                        !(memberExpr.Expression.NodeType == ExpressionType.Convert && ((System.Linq.Expressions.UnaryExpression)memberExpr.Expression).Operand.NodeType == ExpressionType.Parameter))
                        return SimpleMap(datumConverterFactory, expr);

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
                    return SimpleMap(datumConverterFactory, expr);
            }
        }

        #endregion
    }
}

