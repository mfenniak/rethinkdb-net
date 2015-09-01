using System;
using System.Linq.Expressions;
using RethinkDb.Spec;

namespace RethinkDb.Expressions
{
    class ZeroParameterLambda<TReturn> : BaseExpression, IExpressionConverterZeroParameter<TReturn>
    {
        #region Public interface

        private readonly IDatumConverterFactory datumConverterFactory;

        public ZeroParameterLambda(IDatumConverterFactory datumConverterFactory, DefaultExpressionConverterFactory expressionConverterFactory)
            : base(expressionConverterFactory)
        {
            this.datumConverterFactory = datumConverterFactory;
        }

        public Term CreateFunctionTerm(Expression<Func<TReturn>> expression)
        {
            return SimpleMap(datumConverterFactory, expression.Body);
        }

        #endregion
        #region Abstract implementation

        protected override Term RecursiveMap(Expression expression, bool allowMemberInit = false)
        {
            return SimpleMap(datumConverterFactory, expression);
        }

        #endregion
    }
}

