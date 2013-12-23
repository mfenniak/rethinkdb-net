using System;
using System.Linq.Expressions;
using RethinkDb.Spec;

namespace RethinkDb
{
    public interface IExpressionConverterTwoParameter<TParameter1, TParameter2, TReturn>
    {
        Term CreateFunctionTerm(Expression<Func<TParameter1, TParameter2, TReturn>> expression);
    }
}
