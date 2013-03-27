using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    static class ExpressionUtils
    {
        private static Term ConvertBinaryExpressionToTerm<T>(BinaryExpression expr, Term.TermType termType)
        {
            var term = new Term() {
                type = termType
            };
            term.args.Add(MapExpressionToTerm<T>(expr.Left));
            term.args.Add(MapExpressionToTerm<T>(expr.Right));
            return term;
        }

        public static Term MapExpressionToTerm<T>(Expression expr)
        {
            // FIXME: datum converter should be passed in from the connection?
            var datumConverterFactory = DataContractDatumConverterFactory.Instance;

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
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.ADD);
                case ExpressionType.Modulo:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.MOD);
                case ExpressionType.Divide:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.DIV);
                case ExpressionType.Multiply:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.MUL);
                case ExpressionType.Subtract:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.SUB);
                case ExpressionType.Equal:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.EQ);
                case ExpressionType.LessThan:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.LT);
                case ExpressionType.LessThanOrEqual:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.LE);
                case ExpressionType.GreaterThan:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.GT);
                case ExpressionType.GreaterThanOrEqual:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.GE);
                case ExpressionType.AndAlso:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.ALL);
                case ExpressionType.OrElse:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.ANY);
                case ExpressionType.NotEqual:
                    return ConvertBinaryExpressionToTerm<T>((BinaryExpression)expr, Term.TermType.NE);

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

