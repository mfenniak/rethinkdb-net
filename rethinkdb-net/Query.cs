using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IQuery
    {
        Term GenerateTerm(IDatumConverterFactory datumConverterFactory);
    }

    [ImmutableObject(true)]
    public interface ISingleObjectQuery<T> : IQuery
    {
    }

    [ImmutableObject(true)]
    public interface IMutableSingleObjectQuery<T> : ISingleObjectQuery<T>
    {
    }

    [ImmutableObject(true)]
    public interface IDmlQuery : ISingleObjectQuery<DmlResponse>
    {
    }

    [ImmutableObject(true)]
    public interface ISequenceQuery<T> : IQuery
    {
    }

    [ImmutableObject(true)]
    public interface IWriteQuery<T> : IQuery
    {
    }

    public static class Query
    {
        public static DbQuery Db(string db)
        {
            return new DbQuery(db);
        }

        public static DbCreateQuery DbCreate(string db)
        {
            return new DbCreateQuery(db);
        }

        public static DbDropQuery DbDrop(string db)
        {
            return new DbDropQuery(db);
        }

        public static DbListQuery DbList()
        {
            return new DbListQuery();
        }

        public static GetQuery<T> Get<T>(this ISequenceQuery<T> target, string primaryKey, string primaryAttribute = null)
        {
            return new GetQuery<T>(target, primaryKey, primaryAttribute);
        }

        public static FilterQuery<T> Filter<T>(this ISequenceQuery<T> target, Expression<Func<T, bool>> filterExpression)
        {
            return new FilterQuery<T>(target, filterExpression);
        }

        public static UpdateQuery<T> Update<T>(this ISequenceQuery<T> target, Expression<Func<T, T>> updateExpression)
        {
            return new UpdateQuery<T>(target, updateExpression);
        }

        public static UpdateQuery<T> Update<T>(this IMutableSingleObjectQuery<T> target, Expression<Func<T, T>> updateExpression)
        {
            return new UpdateQuery<T>(target, updateExpression);
        }

        public static DeleteQuery<T> Delete<T>(this ISequenceQuery<T> target)
        {
            return new DeleteQuery<T>(target);
        }

        public static DeleteQuery<T> Delete<T>(this IMutableSingleObjectQuery<T> target)
        {
            return new DeleteQuery<T>(target);
        }

        public static ReplaceQuery<T> Replace<T>(this IMutableSingleObjectQuery<T> target, T newObject)
        {
            return new ReplaceQuery<T>(target, newObject);
        }

        public static BetweenQuery<T> Between<T>(this ISequenceQuery<T> target, string leftKey, string rightKey)
        {
            return new BetweenQuery<T>(target, leftKey, rightKey);
        }

        public static BetweenQuery<T> Between<T>(this ISequenceQuery<T> target, double? leftKey, double? rightKey)
        {
            return new BetweenQuery<T>(target, leftKey, rightKey);
        }

        public static CountQuery<T> Count<T>(this ISequenceQuery<T> target)
        {
            return new CountQuery<T>(target);
        }

        public static ExprQuery<T> Expr<T>(T @object)
        {
            return new ExprQuery<T>(@object);
        }

        public static ExprQuery<T> Expr<T>(Expression<Func<T>> objectExpr)
        {
            return new ExprQuery<T>(objectExpr);
        }

        public static ExprSequenceQuery<T> Expr<T>(IEnumerable<T> enumerable)
        {
            return new ExprSequenceQuery<T>(enumerable);
        }

        public static MapQuery<TOriginal, TTarget> Map<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            return new MapQuery<TOriginal, TTarget>(sequenceQuery, mapExpression);
        }

        public static OrderByQuery<T> OrderBy<T>(this ISequenceQuery<T> sequenceQuery, params Expression<Func<T, object>>[] memberReferenceExpressions)
        {
            return new OrderByQuery<T>(sequenceQuery, memberReferenceExpressions);
        }

        public static object Asc(object value)
        {
            throw new InvalidOperationException("This method should never actually be invoked; it should only be used as part of expressions to Query.OrderBy");
        }

        public static object Desc(object value)
        {
            throw new InvalidOperationException("This method should never actually be invoked; it should only be used as part of expressions to Query.OrderBy");
        }

        public static InnerJoinQuery<TLeft, TRight> InnerJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new InnerJoinQuery<TLeft, TRight>(leftQuery, rightQuery, joinPredicate);
        }

        public static ISingleObjectQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction);
        }

        public static ISingleObjectQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction, T @base)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction, @base);
        }
    }
}
