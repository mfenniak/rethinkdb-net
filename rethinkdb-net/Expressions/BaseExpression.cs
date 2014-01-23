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
        /*
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

        private static Lazy<MethodInfo> DateTimeAddTimeSpan = new Lazy<MethodInfo>(() =>
            typeof(DateTime)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "Add")
        );

        private static Lazy<MethodInfo> DateTimeAddMinutes = new Lazy<MethodInfo>(() =>
            typeof(DateTime)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddMinutes")
        );

        private static Lazy<MethodInfo> DateTimeAddSeconds = new Lazy<MethodInfo>(() =>
            typeof(DateTime)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddSeconds")
        );

        private static Lazy<MethodInfo> DateTimeAddHours = new Lazy<MethodInfo>(() =>
            typeof(DateTime)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddHours")
        );

        private static Lazy<MethodInfo> DateTimeAddMilliseconds = new Lazy<MethodInfo>(() =>
            typeof(DateTime)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddMilliseconds")
        );

        private static Lazy<MethodInfo> DateTimeAddTicks = new Lazy<MethodInfo>(() =>
            typeof(DateTime)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddTicks")
        );

        private static Lazy<MethodInfo> DateTimeAddDays = new Lazy<MethodInfo>(() =>
            typeof(DateTime)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddDays")
        );

        private static Lazy<MethodInfo> DateTimeOffsetAddTimeSpan = new Lazy<MethodInfo>(() =>
            typeof(DateTimeOffset)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "Add")
        );

        private static Lazy<MethodInfo> DateTimeOffsetAddMinutes = new Lazy<MethodInfo>(() =>
            typeof(DateTimeOffset)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddMinutes")
        );

        private static Lazy<MethodInfo> DateTimeOffsetAddSeconds = new Lazy<MethodInfo>(() =>
            typeof(DateTimeOffset)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddSeconds")
        );

        private static Lazy<MethodInfo> DateTimeOffsetAddHours = new Lazy<MethodInfo>(() =>
            typeof(DateTimeOffset)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddHours")
        );

        private static Lazy<MethodInfo> DateTimeOffsetAddMilliseconds = new Lazy<MethodInfo>(() =>
            typeof(DateTimeOffset)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddMilliseconds")
        );

        private static Lazy<MethodInfo> DateTimeOffsetAddTicks = new Lazy<MethodInfo>(() =>
            typeof(DateTimeOffset)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddTicks")
        );

        private static Lazy<MethodInfo> DateTimeOffsetAddDays = new Lazy<MethodInfo>(() =>
            typeof(DateTimeOffset)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "AddDays")
        );
        */

        #region Constructor

        protected readonly DefaultExpressionConverterFactory expressionConverterFactory;

        protected BaseExpression(DefaultExpressionConverterFactory expressionConverterFactory)
        {
            this.expressionConverterFactory = expressionConverterFactory;
        }

        #endregion
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

                    /*
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

                        var functionTerm = (Term)createFunctionTermMethod.Invoke(null, new object[] { datumConverterFactory, expressionConverterFactory, predicate });
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
                    else if (method == DateTimeAddTimeSpan.Value)
                    {
                        var addTerm = new Term()
                        {
                            type = Term.TermType.ADD,
                        };
                        addTerm.args.Add(RecursiveMap(callExpression.Object));
                        addTerm.args.Add(RecursiveMap(callExpression.Arguments[0]));
                        return addTerm;
                    }
                    else if (method == DateTimeAddMinutes.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeAddHours.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, TimeSpan.TicksPerHour / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeAddMilliseconds.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, (double)TimeSpan.TicksPerMillisecond / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeAddSeconds.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, 1);
                    }
                    else if (method == DateTimeAddTicks.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, 1.0 / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeAddDays.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeOffsetAddTimeSpan.Value)
                    {
                        var addTerm = new Term()
                        {
                            type = Term.TermType.ADD,
                        };
                        addTerm.args.Add(RecursiveMap(callExpression.Object));
                        addTerm.args.Add(RecursiveMap(callExpression.Arguments[0]));
                        return addTerm;
                    }
                    else if (method == DateTimeOffsetAddMinutes.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, TimeSpan.TicksPerMinute / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeOffsetAddHours.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, TimeSpan.TicksPerHour / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeOffsetAddMilliseconds.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, (double)TimeSpan.TicksPerMillisecond / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeOffsetAddSeconds.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, 1);
                    }
                    else if (method == DateTimeOffsetAddTicks.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, 1.0 / TimeSpan.TicksPerSecond);
                    }
                    else if (method == DateTimeOffsetAddDays.Value)
                    {
                        return ConvertDateTimeAddFunctionToTerm(callExpression, TimeSpan.TicksPerDay / TimeSpan.TicksPerSecond);
                    }
                    else
                    {
                        return AttemptClientSideConversion(datumConverterFactory, expr);
                    }
                    */
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

