using System;

namespace RethinkDb
{
    public static class ReQLExpression
    {
        public static T[] Append<T>(this T[] array, params T[] additionalObjects)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }

        public static T[] Slice<T>(this T[] sequenceQuery, int startIndex)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }

        public static T[] Slice<T>(this T[] sequenceQuery, int startIndex, int endIndex)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }

        public static T[] Slice<T>(this T[] sequenceQuery, int startIndex, int endIndex, Bound leftBound, Bound rightBound)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }
    }
}
