using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace RethinkDb
{
    public static class ReQLExpression
    {
        public static T[] Append<T>(this T[] array, params T[] additionalObjects)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }
    }
}

