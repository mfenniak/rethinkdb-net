using System;
using RethinkDb.Spec;
using System.Linq.Expressions;

namespace RethinkDb
{
    public interface IExpressionConverterOneParameter<TParameter1, TReturn>
    {
        Term CreateFunctionTerm(Expression<Func<TParameter1, TReturn>> expression);
    }
}
