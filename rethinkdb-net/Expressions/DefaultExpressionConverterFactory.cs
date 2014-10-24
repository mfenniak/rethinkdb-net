using System;
using System.Collections.Generic;
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
        private readonly IDictionary<Tuple<Type, Type, ExpressionType>, ExpressionMappingDelegate<BinaryExpression>> binaryExpressionMappingRegistry = new Dictionary<Tuple<Type, Type, ExpressionType>, ExpressionMappingDelegate<BinaryExpression>>();
        private readonly IDictionary<Tuple<Type, ExpressionType>, ExpressionMappingDelegate<UnaryExpression>> unaryExpressionMappingRegistry = new Dictionary<Tuple<Type, ExpressionType>, ExpressionMappingDelegate<UnaryExpression>>();
        private readonly IDictionary<Tuple<Type, string>, ExpressionMappingDelegate<MemberExpression>> memberAccessMappingRegistry = new Dictionary<Tuple<Type, string>, ExpressionMappingDelegate<MemberExpression>>();

        public DefaultExpressionConverterFactory()
        {
            LinqExpressionConverters.RegisterOnConverterFactory(this);
            DateTimeExpressionConverters.RegisterOnConverterFactory(this);
        }

        public void RegisterMethodCallMapping(MethodInfo method, ExpressionMappingDelegate<MethodCallExpression> methodCallMapping)
        {
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();
            methodCallMappingRegistry[method] = methodCallMapping;
        }

        public void RegisterBinaryExpressionMapping<TLeft, TRight>(ExpressionType expressionType, ExpressionMappingDelegate<BinaryExpression> binaryExpressionMapping)
        {
            binaryExpressionMappingRegistry[Tuple.Create(typeof(TLeft), typeof(TRight), expressionType)] = binaryExpressionMapping;
        }

        public void RegisterUnaryExpressionMapping<TExpression>(ExpressionType expressionType, ExpressionMappingDelegate<UnaryExpression> unaryExpressionMapping)
        {
            unaryExpressionMappingRegistry[Tuple.Create(typeof(TExpression), expressionType)] = unaryExpressionMapping;
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
