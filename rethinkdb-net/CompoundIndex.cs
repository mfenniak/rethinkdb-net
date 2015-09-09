using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

namespace RethinkDb
{
    public class CompoundIndex<TRecord>
    {
        private CompoundIndex() { }

        internal Expression<Func<TRecord, object[]>> IndexExpression;

        public static CompoundIndex<TRecord> Make<T1, T2>(Expression<Func<TRecord, T1>> indexExpression1,
                                                                   Expression<Func<TRecord, T2>> indexExpression2)
        {
            return MakeRaw(indexExpression1.Body, indexExpression2.Body);
        }

        public static CompoundIndex<TRecord> Make<T1, T2, T3>(Expression<Func<TRecord, T1>> indexExpression1,
                                                                       Expression<Func<TRecord, T2>> indexExpression2,
                                                                       Expression<Func<TRecord, T3>> indexExpression3)
        {
            return MakeRaw(indexExpression1.Body, indexExpression2.Body, indexExpression3.Body);
        }

        public static CompoundIndex<TRecord> Make<T1, T2, T3, T4>(
            Expression<Func<TRecord, T1>> indexExpression1,
            Expression<Func<TRecord, T2>> indexExpression2,
            Expression<Func<TRecord, T3>> indexExpression3,
            Expression<Func<TRecord, T4>> indexExpression4)
        {
            return MakeRaw(indexExpression1.Body, indexExpression2.Body, indexExpression3.Body, indexExpression4.Body);
        }

        public static CompoundIndex<TRecord> Make<T1, T2, T3, T4, T5>(
            Expression<Func<TRecord, T1>> indexExpression1,
            Expression<Func<TRecord, T2>> indexExpression2,
            Expression<Func<TRecord, T3>> indexExpression3,
            Expression<Func<TRecord, T4>> indexExpression4,
            Expression<Func<TRecord, T5>> indexExpression5)
        {
            return MakeRaw(indexExpression1.Body,
                           indexExpression2.Body,
                           indexExpression3.Body,
                           indexExpression4.Body,
                           indexExpression5.Body);
        }

        public static CompoundIndex<TRecord> Make<T1, T2, T3, T4, T5, T6>(
            Expression<Func<TRecord, T1>> indexExpression1,
            Expression<Func<TRecord, T2>> indexExpression2,
            Expression<Func<TRecord, T3>> indexExpression3,
            Expression<Func<TRecord, T4>> indexExpression4,
            Expression<Func<TRecord, T5>> indexExpression5,
            Expression<Func<TRecord, T6>> indexExpression6)
        {
            return MakeRaw(indexExpression1.Body,
                           indexExpression2.Body,
                           indexExpression3.Body,
                           indexExpression4.Body,
                           indexExpression5.Body,
                           indexExpression6.Body);
        }

        public static CompoundIndex<TRecord> Make<T1, T2, T3, T4, T5, T6, T7>(
            Expression<Func<TRecord, T1>> indexExpression1,
            Expression<Func<TRecord, T2>> indexExpression2,
            Expression<Func<TRecord, T3>> indexExpression3,
            Expression<Func<TRecord, T4>> indexExpression4,
            Expression<Func<TRecord, T5>> indexExpression5,
            Expression<Func<TRecord, T6>> indexExpression6,
            Expression<Func<TRecord, T7>> indexExpression7)
        {
            return MakeRaw(indexExpression1.Body,
                           indexExpression2.Body,
                           indexExpression3.Body,
                           indexExpression4.Body,
                           indexExpression5.Body,
                           indexExpression6.Body,
                           indexExpression7.Body);
        }

        public static CompoundIndex<TRecord> Make<T1, T2, T3, T4, T5, T6, T7, T8>(
            Expression<Func<TRecord, T1>> indexExpression1,
            Expression<Func<TRecord, T2>> indexExpression2,
            Expression<Func<TRecord, T3>> indexExpression3,
            Expression<Func<TRecord, T4>> indexExpression4,
            Expression<Func<TRecord, T5>> indexExpression5,
            Expression<Func<TRecord, T6>> indexExpression6,
            Expression<Func<TRecord, T7>> indexExpression7,
            Expression<Func<TRecord, T8>> indexExpression8)
        {
            return MakeRaw(indexExpression1.Body,
                           indexExpression2.Body,
                           indexExpression3.Body,
                           indexExpression4.Body,
                           indexExpression5.Body,
                           indexExpression6.Body,
                           indexExpression7.Body,
                           indexExpression8.Body);
        }

        public static CompoundIndex<TRecord> Make<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Expression<Func<TRecord, T1>> indexExpression1,
            Expression<Func<TRecord, T2>> indexExpression2,
            Expression<Func<TRecord, T3>> indexExpression3,
            Expression<Func<TRecord, T4>> indexExpression4,
            Expression<Func<TRecord, T5>> indexExpression5,
            Expression<Func<TRecord, T6>> indexExpression6,
            Expression<Func<TRecord, T7>> indexExpression7,
            Expression<Func<TRecord, T8>> indexExpression8,
            Expression<Func<TRecord, T9>> indexExpression9)
        {
            return MakeRaw(indexExpression1.Body,
                           indexExpression2.Body,
                           indexExpression3.Body,
                           indexExpression4.Body,
                           indexExpression5.Body,
                           indexExpression6.Body,
                           indexExpression7.Body,
                           indexExpression8.Body,
                           indexExpression9.Body);
        }

        private static CompoundIndex<TRecord> MakeRaw(params Expression[] indexExpressions)
        {
            var param = Expression.Parameter(typeof(TRecord));
            var visitor = new ParameterReplacingVisitor(param);

            return new CompoundIndex<TRecord>
            {
                IndexExpression = Expression.Lambda<Func<TRecord, object[]>>(Expression.NewArrayInit(typeof(object),
                                                                                                     indexExpressions.Select(expr => Expression.Convert(visitor.Visit(expr), typeof(object)))),
                                                                             param)
            };
        }
    }

    internal class ParameterReplacingVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacingVisitor(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}
