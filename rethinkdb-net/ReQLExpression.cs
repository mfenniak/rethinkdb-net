using System;
using System.Collections.Generic;

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

        public static T Error<T>(string errorText)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }

        public static MatchResponse Match(this string @string, string regexp)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }

        public static Dictionary<TKey, TValue> SetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            throw new NotSupportedException("This method cannot be invoked directly, it can only be used as part of an expression tree.");
        }
    }
}
