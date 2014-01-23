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

