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
        public delegate Term MethodCallMappingDelegate(MethodCallExpression methodCall, RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory);
        public delegate Term BinaryExpressionMappingDelegate(BinaryExpression expr, RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory);
        public delegate Term UnaryExpressionMappingDelegate(UnaryExpression expr, RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory);

        private readonly IDictionary<MethodInfo, MethodCallMappingDelegate> methodCallMappingRegistry = new Dictionary<MethodInfo, MethodCallMappingDelegate>();
        private readonly IDictionary<Tuple<Type, Type, ExpressionType>, BinaryExpressionMappingDelegate> binaryExpressionMappingRegistry = new Dictionary<Tuple<Type, Type, ExpressionType>, BinaryExpressionMappingDelegate>();
        private readonly IDictionary<Tuple<Type, ExpressionType>, UnaryExpressionMappingDelegate> unaryExpressionMappingRegistry = new Dictionary<Tuple<Type, ExpressionType>, UnaryExpressionMappingDelegate>();

        public DefaultExpressionConverterFactory()
        {
            LinqExpressionConverters.RegisterOnConverterFactory(this);
            DateTimeExpressionConverters.RegisterOnConverterFactory(this);
        }

        public void RegisterMethodCallMapping(MethodInfo method, MethodCallMappingDelegate methodCallMapping)
        {
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();
            methodCallMappingRegistry[method] = methodCallMapping;
        }

        public void RegisterBinaryExpressionMapping<TLeft, TRight>(ExpressionType expressionType, BinaryExpressionMappingDelegate binaryExpressionMapping)
        {
            binaryExpressionMappingRegistry[Tuple.Create(typeof(TLeft), typeof(TRight), expressionType)] = binaryExpressionMapping;
        }

        public void RegisterUnaryExpressionMapping<TExpression>(ExpressionType expressionType, UnaryExpressionMappingDelegate unaryExpressionMapping)
        {
            unaryExpressionMappingRegistry[Tuple.Create(typeof(TExpression), expressionType)] = unaryExpressionMapping;
        }

        public bool TryGetMethodCallMapping(MethodInfo method, out MethodCallMappingDelegate methodCallMapping)
        {
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();
            return methodCallMappingRegistry.TryGetValue(method, out methodCallMapping);
        }

        public bool TryGetBinaryExpressionMapping(Type leftType, Type rightType, ExpressionType expressionType, out BinaryExpressionMappingDelegate binaryExpressionMapping)
        {
            return binaryExpressionMappingRegistry.TryGetValue(Tuple.Create(leftType, rightType, expressionType), out binaryExpressionMapping);
        }

        public bool TryGetUnaryExpressionMapping(Type expression, ExpressionType expressionType, out UnaryExpressionMappingDelegate unaryExpressionMapping)
        {
            return unaryExpressionMappingRegistry.TryGetValue(Tuple.Create(expression, expressionType), out unaryExpressionMapping);
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
