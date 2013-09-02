using System;
using System.Linq.Expressions;

namespace RethinkDb
{
    public static class ReQLExpression
    {
        public static T[] Append<T>(this T[] array, params T[] additionalObjects)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }

        public static T[] Filter<T>(this T[] array, Expression<Func<T, bool>> filter)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }
    }
}

