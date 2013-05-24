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

    [ImmutableObject(true)]
    public interface IGroupByReduction<TReductionType>
    {
        Term GenerateReductionObject(IDatumConverterFactory datumConverterFactory);
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

        public static GetQuery<T> Get<T>(this ISequenceQuery<T> target, double primaryKey, string primaryAttribute = null)
        {
            return new GetQuery<T>(target, primaryKey, primaryAttribute);
        }

        public static GetAllQuery<TSequence, TKey> GetAll<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey key, string indexName = null)
        {
            return new GetAllQuery<TSequence, TKey>(target, key, indexName);
        }

        public static FilterQuery<T> Filter<T>(this ISequenceQuery<T> target, Expression<Func<T, bool>> filterExpression)
        {
            return new FilterQuery<T>(target, filterExpression);
        }

        // LINQ-compatible alias for Filter
        public static FilterQuery<T> Where<T>(this ISequenceQuery<T> target, Expression<Func<T, bool>> filterExpression)
        {
            return target.Filter(filterExpression);
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

        public static BetweenQuery<T, string> Between<T>(this ISequenceQuery<T> target, string leftKey, string rightKey)
        {
            return new BetweenQuery<T, string>(target, leftKey, rightKey, null);
        }

        public static BetweenQuery<T, double?> Between<T>(this ISequenceQuery<T> target, double? leftKey, double? rightKey)
        {
            return new BetweenQuery<T, double?>(target, leftKey, rightKey, null);
        }

        public static BetweenQuery<TSequence, TKey> Between<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey leftKey, TKey rightKey, string indexName)
        {
            return new BetweenQuery<TSequence, TKey>(target, leftKey, rightKey, indexName);
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

        // LINQ-compatible alias for Map
        public static MapQuery<TOriginal, TTarget> Select<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            return sequenceQuery.Map(mapExpression);
        }

        public static OrderByQuery<T> OrderBy<T>(this ISequenceQuery<T> sequenceQuery, params Expression<Func<T, object>>[] memberReferenceExpressions)
        {
            return new OrderByQuery<T>(sequenceQuery, memberReferenceExpressions);
        }

        // LINQ-compatible alias for OrderBy
        public static OrderByQuery<T> OrderByDescending<T>(this ISequenceQuery<T> sequenceQuery, params Expression<Func<T, object>>[] memberReferenceExpressions)
        {
            // FIXME: Need to find a way to work Query.Desc() into the member reference expressions...
            throw new NotSupportedException();
        }

        // LINQ-compatible alias for OrderBy
        public static OrderByQuery<T> ThenBy<T>(this ISequenceQuery<T> sequenceQuery, params Expression<Func<T, object>>[] memberReferenceExpressions)
        {
            return sequenceQuery.OrderBy(memberReferenceExpressions);
        }

        // LINQ-compatible alias for OrderBy
        public static OrderByQuery<T> ThenByDescending<T>(this ISequenceQuery<T> sequenceQuery, params Expression<Func<T, object>>[] memberReferenceExpressions)
        {
            return sequenceQuery.OrderByDescending(memberReferenceExpressions);
        }

        public static SkipQuery<T> Skip<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return new SkipQuery<T>(sequenceQuery, count);
        }

        public static LimitQuery<T> Limit<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return new LimitQuery<T>(sequenceQuery, count);
        }

        // LINQ compatible alias for Limit
        public static LimitQuery<T> Take<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return sequenceQuery.Limit(count);
        }

        public static SliceQuery<T> Slice<T>(this ISequenceQuery<T> sequenceQuery, int startIndex, int? endIndex = null)
        {
            return new SliceQuery<T>(sequenceQuery, startIndex, endIndex);
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

        public static OuterJoinQuery<TLeft, TRight> OuterJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new OuterJoinQuery<TLeft, TRight>(leftQuery, rightQuery, joinPredicate);
        }

        public static ZipQuery<TLeft, TRight, TTarget> Zip<TLeft, TRight, TTarget>(this ISequenceQuery<Tuple<TLeft, TRight>> sequenceQuery)
        {
            return new ZipQuery<TLeft, TRight, TTarget>(sequenceQuery);
        }

        public static EqJoinQuery<TLeft, TRight> EqJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, Expression<Func<TLeft, object>> leftMemberReferenceExpression, ISequenceQuery<TRight> rightQuery)
        {
            return new EqJoinQuery<TLeft, TRight>(leftQuery, leftMemberReferenceExpression, rightQuery, null);
        }

        public static EqJoinQuery<TLeft, TRight> EqJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, Expression<Func<TLeft, object>> leftMemberReferenceExpression, ISequenceQuery<TRight> rightQuery, string indexName)
        {
            return new EqJoinQuery<TLeft, TRight>(leftQuery, leftMemberReferenceExpression, rightQuery, indexName);
        }

        public static ReduceQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction);
        }

        public static ReduceQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction, T @base)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction, @base);
        }

        public static NthQuery<T> Nth<T>(this ISequenceQuery<T> sequenceQuery, int index)
        {
            return new NthQuery<T>(sequenceQuery, index);
        }

        public static DistinctQuery<T> Distinct<T>(this ISequenceQuery<T> sequenceQuery)
        {
            return new DistinctQuery<T>(sequenceQuery);
        }

        public static GroupedMapReduceQuery<TOriginal, TGroup, TMap> GroupedMapReduce<TOriginal, TGroup, TMap>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TGroup>> grouping, Expression<Func<TOriginal, TMap>> mapping, Expression<Func<TMap, TMap, TMap>> reduction)
        {
            return new GroupedMapReduceQuery<TOriginal, TGroup, TMap>(sequenceQuery, grouping, mapping, reduction);
        }

        public static GroupedMapReduceQuery<TOriginal, TGroup, TMap> GroupedMapReduce<TOriginal, TGroup, TMap>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TGroup>> grouping, Expression<Func<TOriginal, TMap>> mapping, Expression<Func<TMap, TMap, TMap>> reduction, TMap @base)
        {
            return new GroupedMapReduceQuery<TOriginal, TGroup, TMap>(sequenceQuery, grouping, mapping, reduction, @base);
        }

        public static ConcatMapQuery<TOriginal, TTarget> ConcatMap<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, IEnumerable<TTarget>>> mapping)
        {
            return new ConcatMapQuery<TOriginal, TTarget>(sequenceQuery, mapping);
        }

        public static UnionQuery<T> Union<T>(this ISequenceQuery<T> query1, ISequenceQuery<T> query2)
        {
            return new UnionQuery<T>(query1, query2);
        }

        public static GroupByQuery<TObject, TReductionType, TGroupByType1> GroupBy<TObject, TReductionType, TGroupByType1>(this ISequenceQuery<TObject> sequenceQuery, IGroupByReduction<TReductionType> reductionObject, Expression<Func<TObject, TGroupByType1>> groupMemberReference1)
        {
            return new GroupByQuery<TObject, TReductionType, TGroupByType1>(sequenceQuery, reductionObject, groupMemberReference1);
        }

        public static GroupByQuery<TObject, TReductionType, TGroupByType1, TGroupByType2> GroupBy<TObject, TReductionType, TGroupByType1, TGroupByType2>(this ISequenceQuery<TObject> sequenceQuery, IGroupByReduction<TReductionType> reductionObject, Expression<Func<TObject, TGroupByType1>> groupMemberReference1, Expression<Func<TObject, TGroupByType2>> groupMemberReference2)
        {
            return new GroupByQuery<TObject, TReductionType, TGroupByType1, TGroupByType2>(sequenceQuery, reductionObject, groupMemberReference1, groupMemberReference2);
        }

        public static GroupByQuery<TObject, TReductionType, TGroupByType1, TGroupByType2, TGroupByType3> GroupBy<TObject, TReductionType, TGroupByType1, TGroupByType2, TGroupByType3>(this ISequenceQuery<TObject> sequenceQuery, IGroupByReduction<TReductionType> reductionObject, Expression<Func<TObject, TGroupByType1>> groupMemberReference1, Expression<Func<TObject, TGroupByType2>> groupMemberReference2, Expression<Func<TObject, TGroupByType3>> groupMemberReference3)
        {
            return new GroupByQuery<TObject, TReductionType, TGroupByType1, TGroupByType2, TGroupByType3>(sequenceQuery, reductionObject, groupMemberReference1, groupMemberReference2, groupMemberReference3);
        }

        public static CountReduction Count()
        {
            return CountReduction.Instance;
        }

        public static SumReduction<TObject> Sum<TObject>(Expression<Func<TObject, double>> numericMemberReference)
        {
            return new SumReduction<TObject>(numericMemberReference);
        }

        public static AvgReduction<TObject> Avg<TObject>(Expression<Func<TObject, double>> numericMemberReference)
        {
            return new AvgReduction<TObject>(numericMemberReference);
        }
    }
}
