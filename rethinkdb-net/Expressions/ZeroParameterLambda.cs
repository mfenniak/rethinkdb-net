using RethinkDb.Spec;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RethinkDb.Expressions
{
    class ZeroParameterLambda<TReturn> : BaseExpression, IExpressionConverterZeroParameter<TReturn>
    {
        #region Public interface

        private readonly IDatumConverterFactory datumConverterFactory;
        private readonly IExpressionConverterFactory expressionConverterFactory;

        public ZeroParameterLambda(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            this.datumConverterFactory = datumConverterFactory;
            this.expressionConverterFactory = expressionConverterFactory;
        }

        public Term CreateFunctionTerm(Expression<Func<TReturn>> expression)
        {
            return SimpleMap(datumConverterFactory, expression.Body);
        }

        #endregion
        #region Abstract implementation

        protected override IExpressionConverterFactory ExpressionConverterFactory
        {
            get { return expressionConverterFactory; }
        }

        protected override Term RecursiveMap(Expression expression)
        {
            return SimpleMap(datumConverterFactory, expression);
        }

        protected override Term RecursiveMapMemberInit<TInnerReturn>(Expression expression)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}

