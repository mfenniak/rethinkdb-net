using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface ISingleObjectQuery<T>
    {
        Term GenerateTerm();
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
    public interface ISequenceQuery<T>
    {
        Term GenerateTerm();
    }

    [ImmutableObject(true)]
    public interface IWriteQuery<T>
    {
        Term GenerateTerm(IDatumConverter<T> converter);
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
    }
}
