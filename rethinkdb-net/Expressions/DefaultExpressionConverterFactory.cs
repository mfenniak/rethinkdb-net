using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public class DefaultExpressionConverterFactory : IExpressionConverterFactory
    {
        public delegate Term RecursiveMapDelegate(Expression expression);
        public delegate Term ExpressionMappingDelegate<T>(
            T expression,
            RecursiveMapDelegate recursiveMap,
            IDatumConverterFactory datumConverterFactory,
            IExpressionConverterFactory expressionConverterFactory)
            where T : Expression;

        private readonly IDictionary<MethodInfo, ExpressionMappingDelegate<MethodCallExpression>> methodCallMappingRegistry = new Dictionary<MethodInfo, ExpressionMappingDelegate<MethodCallExpression>>();
        private readonly IDictionary<ConstructorInfo, ExpressionMappingDelegate<NewExpression>> newExpressionMappingRegistry = new Dictionary<ConstructorInfo, ExpressionMappingDelegate<NewExpression>>();
        private readonly IDictionary<Tuple<Type, Type, ExpressionType>, ExpressionMappingDelegate<BinaryExpression>> binaryExpressionMappingRegistry = new Dictionary<Tuple<Type, Type, ExpressionType>, ExpressionMappingDelegate<BinaryExpression>>();
        private readonly IDictionary<Tuple<Type, ExpressionType>, ExpressionMappingDelegate<UnaryExpression>> unaryExpressionMappingRegistry = new Dictionary<Tuple<Type, ExpressionType>, ExpressionMappingDelegate<UnaryExpression>>();
        private readonly IDictionary<Tuple<Type, string>, ExpressionMappingDelegate<MemberExpression>> memberAccessMappingRegistry = new Dictionary<Tuple<Type, string>, ExpressionMappingDelegate<MemberExpression>>();

        public DefaultExpressionConverterFactory()
        {
            LinqExpressionConverters.RegisterOnConverterFactory(this);
            DateTimeExpressionConverters.RegisterOnConverterFactory(this);
            GuidExpressionConverters.RegisterOnConverterFactory(this);
        }

        public void Reset()
        {
            methodCallMappingRegistry.Clear();
            newExpressionMappingRegistry.Clear();
            binaryExpressionMappingRegistry.Clear();
            unaryExpressionMappingRegistry.Clear();
            memberAccessMappingRegistry.Clear();
        }

        public void RegisterMethodCallMapping(MethodInfo method, ExpressionMappingDelegate<MethodCallExpression> methodCallMapping)
        {
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();
            methodCallMappingRegistry[method] = methodCallMapping;
        }

        public void RegisterNewExpressionMapping(ConstructorInfo constructor, ExpressionMappingDelegate<NewExpression> newExpressionMapping)
        {
            newExpressionMappingRegistry[constructor] = newExpressionMapping;
        }

        public void RegisterBinaryExpressionMapping<TLeft, TRight>(ExpressionType expressionType, ExpressionMappingDelegate<BinaryExpression> binaryExpressionMapping)
        {
            binaryExpressionMappingRegistry[Tuple.Create(typeof(TLeft), typeof(TRight), expressionType)] = binaryExpressionMapping;
        }

        public void RegisterBinaryExpressionMapping(Type leftType, Type rightType, ExpressionType expressionType, ExpressionMappingDelegate<BinaryExpression> binaryExpressionMapping)
        {
            binaryExpressionMappingRegistry[Tuple.Create(leftType, rightType, expressionType)] = binaryExpressionMapping;
        }

        public void RegisterUnaryExpressionMapping<TExpression>(ExpressionType expressionType, ExpressionMappingDelegate<UnaryExpression> unaryExpressionMapping)
        {
            unaryExpressionMappingRegistry[Tuple.Create(typeof(TExpression), expressionType)] = unaryExpressionMapping;
        }

        public void RegisterUnaryExpressionMapping(Type operandType, ExpressionType expressionType, ExpressionMappingDelegate<UnaryExpression> unaryExpressionMapping)
        {
            unaryExpressionMappingRegistry[Tuple.Create(operandType, expressionType)] = unaryExpressionMapping;
        }

        public void RegisterMemberAccessMapping(Type targetType, string memberName, ExpressionMappingDelegate<MemberExpression> memberAccessMapping)
        {
            memberAccessMappingRegistry[Tuple.Create(targetType, memberName)] = memberAccessMapping;
        }

        public bool TryGetMethodCallMapping(MethodInfo method, out ExpressionMappingDelegate<MethodCallExpression> methodCallMapping)
        {
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();
            return methodCallMappingRegistry.TryGetValue(method, out methodCallMapping);
        }

        public bool TryGetNewExpressionMapping(ConstructorInfo constructor, out ExpressionMappingDelegate<NewExpression> newExpressionMapping)
        {
            return newExpressionMappingRegistry.TryGetValue(constructor, out newExpressionMapping);
        }

        public bool TryGetBinaryExpressionMapping(Type leftType, Type rightType, ExpressionType expressionType, out ExpressionMappingDelegate<BinaryExpression> binaryExpressionMapping)
        {
            return binaryExpressionMappingRegistry.TryGetValue(Tuple.Create(leftType, rightType, expressionType), out binaryExpressionMapping);
        }

        public bool TryGetUnaryExpressionMapping(Type expression, ExpressionType expressionType, out ExpressionMappingDelegate<UnaryExpression> unaryExpressionMapping)
        {
            return unaryExpressionMappingRegistry.TryGetValue(Tuple.Create(expression, expressionType), out unaryExpressionMapping);
        }

        public bool TryGetMemberAccessMapping(MemberInfo member, out ExpressionMappingDelegate<MemberExpression> memberAccessMapping)
        {
            if (memberAccessMappingRegistry.TryGetValue(Tuple.Create(member.DeclaringType, member.Name), out memberAccessMapping))
                return true;

            if (member.DeclaringType.IsGenericType)
            {
                if (memberAccessMappingRegistry.TryGetValue(Tuple.Create(member.DeclaringType.GetGenericTypeDefinition(), member.Name), out memberAccessMapping))
                    return true;
            }

            return false;
        }

        // RegisterTemplateMapping functions take a LINQ template, extract the expression from the template, and then
        // register a method to create a Term from that template.
        //
        // Example template:
        //     (hours, minutes, seconds) => new TimeSpan(hours, minutes, seconds),
        //
        // Example termConstructor:
        //     (hoursTerm, minutesTerm, secondsTerm) => 
        //         Add(
        //             HoursToSeconds(hours),
        //             MinutesToSeconds(minutesTerm),
        //             secondsTerm
        //         )
        //
        // The goal is to call register a Mapping function that will call termConstructor.  To invoke
        // termConstructor, first all the arguments will need to be RecursiveMap'd, then the termConstructor needs to
        // be invoked.  Will only support the same expression mappings that we otherwise support with Register; eg.
        // method calls, constructors, binary expressions, and unary expressions.

        public void RegisterTemplateMapping<TReturn>(
            Expression<Func<TReturn>> template,
            Func<Term> termConstructor)
        {
            var templateBody = template.Body;
            switch (templateBody.NodeType)
            {
                case ExpressionType.Call:
                    RegisterMethodCallTemplateMapping((MethodCallExpression)templateBody, terms => termConstructor());
                    break;
                case ExpressionType.New:
                    RegisterNewTemplateMapping((NewExpression)templateBody, terms => termConstructor());
                    break;
                default:
                    throw new NotImplementedException("Template did not match supported pattern");
            }
        }

        public void RegisterTemplateMapping<TParameter1, TReturn>(
            Expression<Func<TParameter1, TReturn>> template,
            Func<Term, Term> termConstructor)
        {
            var templateBody = template.Body;
            switch (templateBody.NodeType)
            {
                case ExpressionType.Call:
                    RegisterMethodCallTemplateMapping((MethodCallExpression)templateBody, terms => termConstructor(terms[0]));
                    break;
                case ExpressionType.New:
                    RegisterNewTemplateMapping((NewExpression)templateBody, terms => termConstructor(terms[0]));
                    break;
                default:
                    {
                        var unaryExpression = templateBody as UnaryExpression;
                        if (unaryExpression != null)
                            RegisterUnaryTemplateMapping(unaryExpression, termConstructor);
                        else
                            throw new NotImplementedException("Template did not match supported pattern");
                    }
                    break;
            }
        }

        public void RegisterTemplateMapping<TParameter1, TParameter2, TReturn>(
            Expression<Func<TParameter1, TParameter2, TReturn>> template,
            Func<Term, Term, Term> termConstructor)
        {
            var templateBody = template.Body;
            switch (templateBody.NodeType)
            {
                case ExpressionType.Call:
                    RegisterMethodCallTemplateMapping((MethodCallExpression)templateBody, terms => termConstructor(terms[0], terms[1]));
                    break;
                    case ExpressionType.New:
                    RegisterNewTemplateMapping((NewExpression)templateBody, terms => termConstructor(terms[0], terms[1]));
                    break;
                default:
                    {
                        var binaryExpression = templateBody as BinaryExpression;
                        if (binaryExpression != null)
                            RegisterBinaryTemplateMapping(binaryExpression, termConstructor);
                        else
                            throw new NotImplementedException("Template did not match supported pattern");
                    }
                    break;
            }
        }

        public void RegisterTemplateMapping<TParameter1, TParameter2, TParameter3, TReturn>(
            Expression<Func<TParameter1, TParameter2, TParameter3, TReturn>> template,
            Func<Term, Term, Term, Term> termConstructor)
        {
            var templateBody = template.Body;
            switch (templateBody.NodeType)
            {
                case ExpressionType.Call:
                    RegisterMethodCallTemplateMapping((MethodCallExpression)templateBody, terms => termConstructor(terms[0], terms[1], terms[2]));
                    break;
                case ExpressionType.New:
                    RegisterNewTemplateMapping((NewExpression)templateBody, terms => termConstructor(terms[0], terms[1], terms[2]));
                    break;
                default:
                    throw new NotImplementedException("Template did not match supported pattern");
            }
        }

        public void RegisterTemplateMapping<TParameter1, TParameter2, TParameter3, TParameter4, TReturn>(
            Expression<Func<TParameter1, TParameter2, TParameter3, TParameter4, TReturn>> template,
            Func<Term, Term, Term, Term, Term> termConstructor)
        {
            var templateBody = template.Body;
            switch (templateBody.NodeType)
            {
                case ExpressionType.Call:
                    RegisterMethodCallTemplateMapping(
                        (MethodCallExpression)templateBody,
                        terms => termConstructor(terms[0], terms[1], terms[2], terms[3]));
                    break;
                case ExpressionType.New:
                    RegisterNewTemplateMapping(
                        (NewExpression)templateBody,
                        terms => termConstructor(terms[0], terms[1], terms[2], terms[3]));
                    break;
                default:
                    throw new NotImplementedException("Template did not match supported pattern");
            }
        }

        public void RegisterTemplateMapping<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, TReturn>(
            Expression<Func<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, TReturn>> template,
            Func<Term, Term, Term, Term, Term, Term> termConstructor)
        {
            var templateBody = template.Body;
            switch (templateBody.NodeType)
            {
                case ExpressionType.Call:
                    RegisterMethodCallTemplateMapping(
                        (MethodCallExpression)templateBody,
                        terms => termConstructor(terms[0], terms[1], terms[2], terms[3], terms[4]));
                    break;
                case ExpressionType.New:
                    RegisterNewTemplateMapping(
                        (NewExpression)templateBody,
                        terms => termConstructor(terms[0], terms[1], terms[2], terms[3], terms[4]));
                    break;
                default:
                    throw new NotImplementedException("Template did not match supported pattern");
            }
        }

        public void RegisterTemplateMapping<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, TParameter6, TReturn>(
            Expression<Func<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, TParameter6, TReturn>> template,
            Func<Term, Term, Term, Term, Term, Term, Term> termConstructor)
        {
            var templateBody = template.Body;
            switch (templateBody.NodeType)
            {
                case ExpressionType.Call:
                        RegisterMethodCallTemplateMapping(
                            (MethodCallExpression)templateBody,
                            terms => termConstructor(terms[0], terms[1], terms[2], terms[3], terms[4], terms[5]));
                        break;
                    case ExpressionType.New:
                        RegisterNewTemplateMapping(
                            (NewExpression)templateBody,
                            terms => termConstructor(terms[0], terms[1], terms[2], terms[3], terms[4], terms[6]));
                        break;
                    default:
                        throw new NotImplementedException("Template did not match supported pattern");
            }
        }

        public void RegisterTemplateMapping<TFunc>(
            Expression<TFunc> template,
            Func<Term[], Term> termConstructor)
        {
            var templateBody = template.Body;
            switch (templateBody.NodeType)
            {
                case ExpressionType.Call:
                        RegisterMethodCallTemplateMapping((MethodCallExpression)templateBody, termConstructor);
                        break;
                case ExpressionType.New:
                        RegisterNewTemplateMapping((NewExpression)templateBody, termConstructor);
                        break;
                    default:
                        throw new NotImplementedException("Template did not match supported pattern");
            }
        }

        private void RegisterMethodCallTemplateMapping(MethodCallExpression templateMethodCall, Func<Term[], Term> termConstructor)
        {
            var templateMethod = templateMethodCall.Method;

            DefaultExpressionConverterFactory.ExpressionMappingDelegate<MethodCallExpression> del = delegate(
                MethodCallExpression queryExpression,
                DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap,
                IDatumConverterFactory datumConverterFactory,
                IExpressionConverterFactory internalExpressionConverterFactory)
            {
                if (queryExpression.Arguments.Count != templateMethodCall.Arguments.Count)
                    throw new InvalidOperationException("Unexpected mismatch between template method and query method");

                // If this is an instance method, we assume the first parameter is the object.  Technically we could
                // figure this out by inspecting templateMethodCall.Object, but this is a simple assumption for now.
                var mappedArguments = queryExpression.Arguments.Select(expr => recursiveMap(expr));
                if (templateMethodCall.Object != null)
                {
                    return termConstructor(new Term[] { recursiveMap(queryExpression.Object) }.Concat(mappedArguments).ToArray());
                }
                else
                {
                    return termConstructor(mappedArguments.ToArray());
                }
            };

            RegisterMethodCallMapping(templateMethod, del);
        }

        private void RegisterNewTemplateMapping(NewExpression templateNew, Func<Term[], Term> termConstructor)
        {
            var templateConstructor = templateNew.Constructor;

            DefaultExpressionConverterFactory.ExpressionMappingDelegate<NewExpression> del = delegate(
                NewExpression queryExpression,
                DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap,
                IDatumConverterFactory datumConverterFactory,
                IExpressionConverterFactory internalExpressionConverterFactory)
            {
                if (queryExpression.Arguments.Count != templateConstructor.GetParameters().Length)
                    throw new InvalidOperationException("Unexpected mismatch between template method and query method");
                var mappedArguments = queryExpression.Arguments.Select(expr => recursiveMap(expr));
                return termConstructor(mappedArguments.ToArray());
            };

            RegisterNewExpressionMapping(templateConstructor, del);
        }

        private void RegisterBinaryTemplateMapping(BinaryExpression templateBinary, Func<Term, Term, Term> termConstructor)
        {
            var leftType = templateBinary.Left.Type;
            var expressionType = templateBinary.NodeType;
            var rightType = templateBinary.Right.Type;

            DefaultExpressionConverterFactory.ExpressionMappingDelegate<BinaryExpression> del = delegate(
                BinaryExpression queryExpression,
                DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap,
                IDatumConverterFactory datumConverterFactory,
                IExpressionConverterFactory internalExpressionConverterFactory)
            {
                return termConstructor(recursiveMap(queryExpression.Left), recursiveMap(queryExpression.Right));
            };

            RegisterBinaryExpressionMapping(leftType, rightType, expressionType, del);
        }

        private void RegisterUnaryTemplateMapping(UnaryExpression templateUnary, Func<Term, Term> termConstructor)
        {
            var expressionType = templateUnary.NodeType;
            var innerType = templateUnary.Operand.Type;

            DefaultExpressionConverterFactory.ExpressionMappingDelegate<UnaryExpression> del = delegate(
                UnaryExpression queryExpression,
                DefaultExpressionConverterFactory.RecursiveMapDelegate recursiveMap,
                IDatumConverterFactory datumConverterFactory,
                IExpressionConverterFactory internalExpressionConverterFactory)
            {
                return termConstructor(recursiveMap(queryExpression.Operand));
            };

            RegisterUnaryExpressionMapping(innerType, expressionType, del);
        }
        #region IExpressionConverterFactory implementation

        public IExpressionConverterZeroParameter<TReturn> CreateExpressionConverter<TReturn>(IDatumConverterFactory datumConverterFactory)
        {
            return new ZeroParameterLambda<TReturn>(datumConverterFactory, this);
        }

        public IExpressionConverterOneParameter<TParameter1, TReturn> CreateExpressionConverter<TParameter1, TReturn>(IDatumConverterFactory datumConverterFactory)
        {
            return new SingleParameterLambda<TParameter1, TReturn>(datumConverterFactory, this);
        }

        public IExpressionConverterTwoParameter<TParameter1, TParameter2, TReturn> CreateExpressionConverter<TParameter1, TParameter2, TReturn>(IDatumConverterFactory datumConverterFactory)
        {
            return new TwoParameterLambda<TParameter1, TParameter2, TReturn>(datumConverterFactory, this);
        }

        #endregion
    }
}
