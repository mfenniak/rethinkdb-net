using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class UpdateQuery<T> : IWriteQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceTerm;
        private readonly ISingleObjectQuery<T> singleObjectTerm;
        private readonly Expression<Func<T, T>> updateExpression;

        public UpdateQuery(ISequenceQuery<T> tableTerm, Expression<Func<T, T>> updateExpression)
        {
            this.sequenceTerm = tableTerm;
            this.updateExpression = updateExpression;
        }

        public UpdateQuery(ISingleObjectQuery<T> singleObjectTerm, Expression<Func<T, T>> updateExpression)
        {
            this.singleObjectTerm = singleObjectTerm;
            this.updateExpression = updateExpression;
        }

        public Term GenerateTerm(IDatumConverter<T> datumConverter)
        {
            var updateTerm = new Term()
            {
                type = Term.TermType.UPDATE,
            };
            if (singleObjectTerm != null)
                updateTerm.args.Add(singleObjectTerm.GenerateTerm());
            else
                updateTerm.args.Add(sequenceTerm.GenerateTerm());

            if (updateExpression.NodeType != ExpressionType.Lambda)
                throw new NotSupportedException("Unsupported expression type");

            var body = updateExpression.Body;
            if (body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException("Only MemberInit expression type is supported inside lambda");

            var memberInit = (MemberInitExpression)body;
            if (memberInit.Type != typeof(T))
                throw new InvalidOperationException("Only expression types matching the table type are supported");
            else if (memberInit.NewExpression.Arguments.Count != 0)
                throw new NotSupportedException("Constructors will not work here, only field member initialization");

            updateTerm.args.Add(MapMemberInitToTerm(memberInit));

            return updateTerm;
        }

        private Term MapMemberInitToTerm(MemberInitExpression memberInit)
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

            var makeObjTerm = new Term() {
                type = Term.TermType.MAKE_OBJ,
            };
            funcTerm.args.Add(makeObjTerm);

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

            return funcTerm;
        }

        private Term.AssocPair MapMemberAssignmentToMakeObjArg(MemberAssignment memberAssignment)
        {
            var retval = new Term.AssocPair();

            var datumConverter = DataContractDatumConverterFactory.Instance.Get<T>();

            var fieldConverter = datumConverter as IObjectDatumConverter;
            if (fieldConverter == null)
                throw new NotSupportedException("Cannot map member assignments into ReQL without implementing IObjectDatumConverter");

            retval.key = fieldConverter.GetDatumFieldName(memberAssignment.Member);
            retval.val = MapExpressionToTerm(memberAssignment.Expression);

            return retval;
        }

        private Term ConvertBinaryExpressionToTerm(BinaryExpression expr, Term.TermType termType)
        {
            var term = new Term() {
                type = termType
            };
            term.args.Add(MapExpressionToTerm(expr.Left));
            term.args.Add(MapExpressionToTerm(expr.Right));
            return term;
        }

        private Term MapExpressionToTerm(Expression expr)
        {
            // FIXME: datum converter should be passed in from the connection?
            var datumConverterFactory = DataContractDatumConverterFactory.Instance;

            switch (expr.NodeType)
            {
                case ExpressionType.Constant:
                {
                    var constantExpression = (ConstantExpression)expr;

                    // FIXME: could avoid reflection here easily if there were non-generic methods on IDatumConverterFactory and IDatumConverter
                    var conversionMethod = typeof(UpdateQuery<T>).GetMethod("ReflectedConstantConversion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    conversionMethod = conversionMethod.MakeGenericMethod(new Type[] { constantExpression.Type });

                    var datum = (Datum)conversionMethod.Invoke(null, new object[] { datumConverterFactory, constantExpression.Value });
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

                    // FIXME: datum converter should be passed in from the connection?
                    var datumConverter = DataContractDatumConverterFactory.Instance.Get<T>();
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
                    throw new NotSupportedException(String.Format("Unsupported expression type: {0}", expr.NodeType));
            }
        }

        private static Datum ReflectedConstantConversion<TInnerType>(IDatumConverterFactory datumFactory, TInnerType obj)
        {
            var converter = datumFactory.Get<TInnerType>();
            return converter.ConvertObject(obj);
        }
    }
}
