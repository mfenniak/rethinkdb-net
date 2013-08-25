using RethinkDb.Spec;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RethinkDb.Expressions
{
    abstract class BaseExpression
    {
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

                    // FIXME: could avoid reflection here easily if there were non-generic methods on IDatumConverterFactory and IDatumConverter
                    var conversionMethod = typeof(BaseExpression).GetMethod("ReflectedConstantConversion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
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

                    if (method.GetGenericMethodDefinition() == typeof(ReQLExpression).GetMethod("Append", BindingFlags.Static | BindingFlags.Public))
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
                                Console.WriteLine("recursiveMapMethod 1 = {0}", recursiveMapMethod);
                                recursiveMapMethod = recursiveMapMethod.MakeGenericMethod(new Type[] { memberInit.Type });
                                Console.WriteLine("recursiveMapMethod 2 = {0}", recursiveMapMethod);
                                newTerm.args.Add((Term)recursiveMapMethod.Invoke(this, new object[] { memberInit }));
                            }
                            else
                                throw new NotSupportedException(String.Format("Expected ReQLExpression.Append to contain MemberInit additions, but was: {0}", datumExpression.NodeType));

                            term = newTerm;
                        }

                        return term;
                    }
                    else
                        throw new NotSupportedException(String.Format("Expected ReQLExpression method call, but was {0}", method));
                }

                default:
                {
                    var conversionMethod = typeof(BaseExpression).GetMethod("ReflectedExpressionClientSideConversion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    conversionMethod = conversionMethod.MakeGenericMethod(new Type[] { expr.Type });

                    var datum = (Datum)conversionMethod.Invoke(null, new object[] { datumConverterFactory, expr });
                    return new Term() {
                        type = Term.TermType.DATUM,
                        datum = datum
                    };
                }
            }
        }

        private static Datum ReflectedConstantConversion<TInnerType>(IDatumConverterFactory datumFactory, TInnerType obj)
        {
            var converter = datumFactory.Get<TInnerType>();
            return converter.ConvertObject(obj);
        }

        private static Datum ReflectedExpressionClientSideConversion<TInnerType>(IDatumConverterFactory datumFactory, Expression expr)
        {
            var converter = datumFactory.Get<TInnerType>();
            var clientSideFunc = Expression.Lambda<Func<TInnerType>>(expr).Compile();
            return converter.ConvertObject(clientSideFunc());
        }

        #endregion
    }
}

