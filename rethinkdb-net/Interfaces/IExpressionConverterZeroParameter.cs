using System;
using RethinkDb.Spec;
using System.Linq.Expressions;

namespace RethinkDb
{
    public interface IExpressionConverterZeroParameter<TReturn>
    {
        Term CreateFunctionTerm(Expression<Func<TReturn>> expression);
    }
}
