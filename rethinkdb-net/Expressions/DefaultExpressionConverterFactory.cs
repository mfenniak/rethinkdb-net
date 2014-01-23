using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    public class DefaultExpressionConverterFactory : IExpressionConverterFactory
    {
        public delegate Term RecursiveMapDelegate(Expression expression);
        public delegate Term MethodCallMappingDelegate(MethodCallExpression methodCall, RecursiveMapDelegate recursiveMap, IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory);

        private readonly IDictionary<MethodInfo, MethodCallMappingDelegate> methodCallMappingRegistry = new Dictionary<MethodInfo, MethodCallMappingDelegate>();

        public DefaultExpressionConverterFactory()
        {
            LinqExpressionConverters.RegisterOnConverterFactory(this);
        }

        public void RegisterMethodCallMapping(MethodInfo method, MethodCallMappingDelegate methodCallMapping)
        {
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();
            methodCallMappingRegistry[method] = methodCallMapping;
        }

        public bool TryGetMethodCallMapping(MethodInfo method, out MethodCallMappingDelegate methodCallMapping)
        {
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();
            return methodCallMappingRegistry.TryGetValue(method, out methodCallMapping);
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
