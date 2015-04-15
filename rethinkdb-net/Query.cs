using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using RethinkDb.QueryTerm;
using RethinkDb.Spec;

namespace RethinkDb
{
    public static class Query
    {
        #region Database Operations

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

        #endregion
        #region Table Operations

        public static TableQuery<T> Table<T>(this IDatabaseQuery target, string table, bool useOutdated = false)
        {
            return new TableQuery<T>(target, table, useOutdated);
        }

        public static TableCreateQuery TableCreate(this IDatabaseQuery target, string table, string datacenter = null, string primaryKey = null, double? cacheSize = null)
        {
            return new TableCreateQuery(target, table, datacenter, primaryKey, cacheSize);
        }

        public static TableDropQuery TableDrop(this IDatabaseQuery target, string table)
        {
            return new TableDropQuery(target, table);
        }

        public static TableListQuery TableList(this IDatabaseQuery target)
        {
            return new TableListQuery(target);
        }

        public static IndexCreateQuery<T, TIndexExpression> IndexCreate<T, TIndexExpression>(this ITableQuery<T> target, string indexName, Expression<Func<T, TIndexExpression>> indexExpression, bool multiIndex = false)
        {
            return new IndexCreateQuery<T, TIndexExpression>(target, indexName, indexExpression, multiIndex);
        }

        public static IndexWaitQuery<T> IndexWait<T>(this ITableQuery<T> target, params string[] indexNames)
        {
            return new IndexWaitQuery<T>(target, indexNames);
        }

        public static IndexStatusQuery<T> IndexStatus<T>(this ITableQuery<T> target, params string[] indexNames)
        {
            return new IndexStatusQuery<T>(target, indexNames);
        }

        public static IndexListQuery<T> IndexList<T>(this ITableQuery<T> target)
        {
            return new IndexListQuery<T>(target);
        }

        public static IndexDropQuery<T> IndexDrop<T>(this ITableQuery<T> target, string indexName)
        {
            return new IndexDropQuery<T>(target, indexName);
        }

        public static IIndex<TRecord, TIndex> IndexDefine<TRecord, TIndex>(this ITableQuery<TRecord> table, string name, Expression<Func<TRecord, TIndex>> indexAccessor)
        {
            return new Index<TRecord, TIndex>(table, name, indexAccessor);
        }

        public static IMultiIndex<TRecord, TIndex> IndexDefineMulti<TRecord, TIndex>(this ITableQuery<TRecord> table, string name, Expression<Func<TRecord, IEnumerable<TIndex>>> indexAccessor)
        {
            return new MultiIndex<TRecord, TIndex>(table, name, indexAccessor);
        }

        public static IndexCreateQuery<TRecord, TIndex> IndexCreate<TRecord, TIndex>(this IIndex<TRecord, TIndex> index)
        {
            return index.Table.IndexCreate(index.Name, index.IndexAccessor, false);
        }

        public static IndexCreateQuery<TRecord, IEnumerable<TIndex>> IndexCreate<TRecord, TIndex>(this IMultiIndex<TRecord, TIndex> index)
        {
            return index.Table.IndexCreate(index.Name, index.IndexAccessor, true);
        }

        public static IndexStatusQuery<TRecord> IndexStatus<TRecord, TIndex>(this IBaseIndex<TRecord, TIndex> index)
        {
            // FIXME: Since this overload only takes a single index name, it'd be nice if we could return an
            // IScalarQuery... but we don't currently have a query op that takes a sequence and returns a
            // single item.
            return index.Table.IndexStatus(index.Name);
        }

        public static IndexWaitQuery<TRecord> IndexWait<TRecord, TIndex>(this IBaseIndex<TRecord, TIndex> index)
        {
            // FIXME: Since this overload only takes a single index name, it'd be nice if we could return an
            // IScalarQuery... but we don't currently have a query op that takes a sequence and returns a
            // single item.
            return index.Table.IndexWait(index.Name);
        }

        public static IndexDropQuery<TRecord> IndexDrop<TRecord, TIndex>(this IBaseIndex<TRecord, TIndex> index)
        {
            return index.Table.IndexDrop(index.Name);
        }

        public static InsertQuery<T> Insert<T>(this ITableQuery<T> target, T @object, Conflict conflict = Conflict.Error)
        {
            return new InsertQuery<T>(target, new T[] { @object }, conflict);
        }

        public static InsertQuery<T> Insert<T>(this ITableQuery<T> target, IEnumerable<T> @objects, Conflict conflict = Conflict.Error)
        {
            return new InsertQuery<T>(target, @objects, conflict);
        }

        public static ChangesQuery<TRecord> Changes<TRecord>(this IChangefeedCompatibleQuery<TRecord> target)
        {
            return new ChangesQuery<TRecord>(target);
        }

        #endregion
        #region Query Operations

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
            return new GetAllQuery<TSequence, TKey>(target, new TKey[] { key }, indexName);
        }

        public static GetAllQuery<TSequence, TKey> GetAll<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey[] keys, string indexName = null)
        {
            return new GetAllQuery<TSequence, TKey>(target, keys, indexName);
        }

        public static GetAllQuery<TSequence, TKey> GetAll<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey key, IBaseIndex<TSequence, TKey> index)
        {
            return target.GetAll(key, indexName: index.Name);
        }

        public static GetAllQuery<TSequence, TKey> GetAll<TSequence, TKey>(this IBaseIndex<TSequence, TKey> index, params TKey[] keys)
        {
            return index.Table.GetAll(keys: keys, indexName: index.Name);
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

        public static UpdateQuery<T> Update<T>(this ISequenceQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateQuery<T>(target, updateExpression, nonAtomic);
        }

        public static UpdateQuery<T> Update<T>(this IMutableSingleObjectQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateQuery<T>(target, updateExpression, nonAtomic);
        }

        public static UpdateAndReturnValueQuery<T> UpdateAndReturnChanges<T>(this IMutableSingleObjectQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateAndReturnValueQuery<T>(target, updateExpression, nonAtomic);
        }

        public static DeleteQuery<T> Delete<T>(this ISequenceQuery<T> target)
        {
            return new DeleteQuery<T>(target);
        }

        public static DeleteQuery<T> Delete<T>(this IMutableSingleObjectQuery<T> target)
        {
            return new DeleteQuery<T>(target);
        }

        public static DeleteAndReturnValueQuery<T> DeleteAndReturnChanges<T>(this IMutableSingleObjectQuery<T> target)
        {
            return new DeleteAndReturnValueQuery<T>(target);
        }

        public static ReplaceQuery<T> Replace<T>(this IMutableSingleObjectQuery<T> target, T newObject, bool nonAtomic = false)
        {
            return new ReplaceQuery<T>(target, newObject, nonAtomic);
        }

        public static ReplaceAndReturnValueQuery<T> ReplaceAndReturnChanges<T>(this IMutableSingleObjectQuery<T> target, T newObject, bool nonAtomic = false)
        {
            return new ReplaceAndReturnValueQuery<T>(target, newObject, nonAtomic);
        }

        public static BetweenQuery<TSequence, TKey> Between<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey leftKey, TKey rightKey, string indexName = null, Bound leftBound = Bound.Closed, Bound rightBound = Bound.Open)
        {
            return new BetweenQuery<TSequence, TKey>(target, leftKey, rightKey, indexName, leftBound, rightBound);
        }

        public static BetweenQuery<TSequence, TKey> Between<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey leftKey, TKey rightKey, IBaseIndex<TSequence, TKey> index, Bound leftBound = Bound.Closed, Bound rightBound = Bound.Open)
        {
            return target.Between(leftKey, rightKey, index.Name, leftBound, rightBound);
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

        public static OrderByQuery<T> OrderBy<T>(
            this ISequenceQuery<T> sequenceQuery,
            Expression<Func<T, object>> memberReferenceExpression,
            OrderByDirection direction = OrderByDirection.Ascending)
        {
            return new OrderByQuery<T>(sequenceQuery, new OrderByTerm<T> {
                Expression = memberReferenceExpression,
                Direction = direction
            });
        }

        public static OrderByIndexQuery<T> OrderBy<T>(
            this ISequenceQuery<T> sequenceQuery,
            string indexName,
            OrderByDirection direction = OrderByDirection.Ascending)
        {
            return new OrderByIndexQuery<T>(sequenceQuery, new OrderByTerm<T> {
                Direction = direction,
                IndexName = indexName,
            });
        }

        public static OrderByIndexQuery<T> OrderBy<T, TIndexType>(
            this ISequenceQuery<T> sequenceQuery,
            IIndex<T, TIndexType> index,
            OrderByDirection direction = OrderByDirection.Ascending)
        {
            return new OrderByIndexQuery<T>(sequenceQuery, new OrderByTerm<T> {
                Direction = direction,
                IndexName = index.Name,
            });
        }

        // LINQ-compatible alias for OrderBy
        public static OrderByQuery<T> OrderByDescending<T>(
            this ISequenceQuery<T> sequenceQuery,
            Expression<Func<T, object>> memberReferenceExpression)
        {
            return sequenceQuery.OrderBy(memberReferenceExpression, OrderByDirection.Descending);
        }
        public static OrderByIndexQuery<T> OrderByDescending<T>(
            this ISequenceQuery<T> sequenceQuery,
            string indexName)
        {
            return sequenceQuery.OrderBy(indexName, OrderByDirection.Descending);
        }
        public static OrderByIndexQuery<T> OrderByDescending<T, TIndexType>(
            this ISequenceQuery<T> sequenceQuery,
            IIndex<T, TIndexType> index)
        {
            return sequenceQuery.OrderBy(index, OrderByDirection.Descending);
        }

        public static OrderByQuery<T> ThenBy<T>(
            this IOrderedSequenceQuery<T> orderByQuery,
            Expression<Func<T, object>> memberReferenceExpression,
            OrderByDirection direction = OrderByDirection.Ascending)
        {
            return new OrderByQuery<T>(
                orderByQuery.SequenceQuery,
                orderByQuery.OrderByMembers.Concat(
                    Enumerable.Repeat(
                        new OrderByTerm<T> {
                            Expression = memberReferenceExpression,
                            Direction = direction,
                            IndexName = null
                        }, 1)).ToArray());
        }

        // LINQ-compatible alias for OrderBy
        public static OrderByQuery<T> ThenByDescending<T>(
            this IOrderedSequenceQuery<T> orderByQuery,
            Expression<Func<T, object>> memberReferenceExpression)
        {
            return orderByQuery.ThenBy(memberReferenceExpression, OrderByDirection.Descending);
        }

        public static SkipQuery<T> Skip<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return new SkipQuery<T>(sequenceQuery, count);
        }

        public static LimitQueryChangefeedCompatible<T> Limit<T>(this IOrderByIndexQuery<T> orderByIndexQuery, int count)
        {
            return new LimitQueryChangefeedCompatible<T>(orderByIndexQuery, count);
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

        public static InnerJoinQuery<TLeft, TRight, Tuple<TLeft, TRight>> InnerJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new InnerJoinQuery<TLeft, TRight, Tuple<TLeft, TRight>>(leftQuery, rightQuery, joinPredicate);
        }

        public static InnerJoinQuery<TLeft, TRight, TResult> InnerJoin<TLeft, TRight, TResult>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new InnerJoinQuery<TLeft, TRight, TResult>(leftQuery, rightQuery, joinPredicate);
        }

        public static OuterJoinQuery<TLeft, TRight, Tuple<TLeft, TRight>> OuterJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new OuterJoinQuery<TLeft, TRight, Tuple<TLeft, TRight>>(leftQuery, rightQuery, joinPredicate);
        }

        public static OuterJoinQuery<TLeft, TRight, TResult> OuterJoin<TLeft, TRight, TResult>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new OuterJoinQuery<TLeft, TRight, TResult>(leftQuery, rightQuery, joinPredicate);
        }

        public static ZipQuery<Tuple<TLeft, TRight>, TTarget> Zip<TLeft, TRight, TTarget>(this ISequenceQuery<Tuple<TLeft, TRight>> sequenceQuery)
        {
            return new ZipQuery<Tuple<TLeft, TRight>, TTarget>(sequenceQuery);
        }

        public static ZipQuery<TJoinedType, TTarget> Zip<TJoinedType, TTarget>(this ISequenceQuery<TJoinedType> sequenceQuery)
        {
            return new ZipQuery<TJoinedType, TTarget>(sequenceQuery);
        }

        public static EqJoinQuery<TLeft, TRight, Tuple<TLeft, TRight>> EqJoin<TLeft, TRight>(
            this ISequenceQuery<TLeft> leftQuery,
            Expression<Func<TLeft, object>> leftMemberReferenceExpression,
            ISequenceQuery<TRight> rightQuery,
            string indexName = null)
        {
            return new EqJoinQuery<TLeft, TRight, Tuple<TLeft, TRight>>(leftQuery, leftMemberReferenceExpression, rightQuery, indexName);
        }

        public static EqJoinQuery<TLeft, TRight, TResult> EqJoin<TLeft, TRight, TResult>(
            this ISequenceQuery<TLeft> leftQuery,
            Expression<Func<TLeft, object>> leftMemberReferenceExpression,
            ISequenceQuery<TRight> rightQuery,
            string indexName = null)
        {
            return new EqJoinQuery<TLeft, TRight, TResult>(leftQuery, leftMemberReferenceExpression, rightQuery, indexName);
        }

        public static EqJoinQuery<TLeft, TRight, Tuple<TLeft, TRight>> EqJoin<TLeft, TRight, TIndexType>(
            this ISequenceQuery<TLeft> leftQuery,
            Expression<Func<TLeft, object>> leftMemberReferenceExpression,
            ISequenceQuery<TRight> rightQuery,
            IBaseIndex<TRight, TIndexType> index)
        {
            return leftQuery.EqJoin(leftMemberReferenceExpression, rightQuery, index.Name);
        }

        public static EqJoinQuery<TLeft, TRight, TResult> EqJoin<TLeft, TRight, TResult, TIndexType>(
            this ISequenceQuery<TLeft> leftQuery,
            Expression<Func<TLeft, object>> leftMemberReferenceExpression,
            ISequenceQuery<TRight> rightQuery,
            IBaseIndex<TRight, TIndexType> index)
        {
            return leftQuery.EqJoin<TLeft, TRight, TResult>(leftMemberReferenceExpression, rightQuery, index.Name);
        }

        public static ReduceQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction);
        }

        public static NthQuery<T> Nth<T>(this ISequenceQuery<T> sequenceQuery, int index)
        {
            return new NthQuery<T>(sequenceQuery, index);
        }

        public static DistinctQuery<T> Distinct<T>(this ISequenceQuery<T> sequenceQuery)
        {
            return new DistinctQuery<T>(sequenceQuery);
        }

        public static ConcatMapQuery<TOriginal, TTarget> ConcatMap<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, IEnumerable<TTarget>>> mapping)
        {
            return new ConcatMapQuery<TOriginal, TTarget>(sequenceQuery, mapping);
        }

        public static UnionQuery<T> Union<T>(this ISequenceQuery<T> query1, ISequenceQuery<T> query2)
        {
            return new UnionQuery<T>(query1, query2);
        }

        [Obsolete("Use DateTimeOffset.UtcNow instead")]
        public static NowQuery<DateTimeOffset> Now()
        {
            return new NowQuery<DateTimeOffset>();
        }

        public static NowQuery<TResult> Now<TResult>()
        {
            return new NowQuery<TResult>();
        }

        public static SampleQuery<T> Sample<T>(this ISequenceQuery<T> target, int count)
        {
            return new SampleQuery<T>(target, count);
        }

        public static HasFieldsSequenceQuery<T> HasFields<T>(this ISequenceQuery<T> target, params Expression<Func<T, object>>[] fields)
        {
            return new HasFieldsSequenceQuery<T>(target, fields);
        }

        public static HasFieldsSingleObjectQuery<T> HasFields<T>(this ISingleObjectQuery<T> target, params Expression<Func<T, object>>[] fields)
        {
            return new HasFieldsSingleObjectQuery<T>(target, fields);
        }

        #region Grouping and Aggregation

        public static GroupByIndexQuery<TRecord, TIndexType> Group<TRecord, TIndexType>(
            // Can only use indexName on Group on a TABLE, not any arbitrary sequence
            this ITableQuery<TRecord> table,
            string indexName
            )
        {
            return new GroupByIndexQuery<TRecord, TIndexType>(table, indexName);
        }

        public static GroupByIndexQuery<TRecord, TIndexType> Group<TRecord, TIndexType>(
            // Can only use indexName on Group on a TABLE, not any arbitrary sequence
            this ITableQuery<TRecord> table,
            IIndex<TRecord, TIndexType> index
            )
        {
            return table.Group<TRecord, TIndexType>(index.Name);
        }

        public static GroupByFunctionQuery<TRecord, TKey> Group<TRecord, TKey>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TKey>> key
            )
        {
            return new GroupByFunctionQuery<TRecord, TKey>(sequenceQuery, key);
        }

        public static GroupByFunctionQuery<TRecord, TKey1, TKey2, Tuple<TKey1, TKey2>> Group<TRecord, TKey1, TKey2>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TKey1>> key1,
            Expression<Func<TRecord, TKey2>> key2
            )
        {
            return new GroupByFunctionQuery<TRecord, TKey1, TKey2, Tuple<TKey1, TKey2>>(sequenceQuery, key1, key2);
        }

        public static GroupByFunctionQuery<TRecord, TKey1, TKey2, TGroupingKey> Group<TRecord, TKey1, TKey2, TGroupingKey>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TKey1>> key1,
            Expression<Func<TRecord, TKey2>> key2
            )
        {
            return new GroupByFunctionQuery<TRecord, TKey1, TKey2, TGroupingKey>(sequenceQuery, key1, key2);
        }

        public static GroupByFunctionQuery<TRecord, TKey1, TKey2, TKey3, Tuple<TKey1, TKey2, TKey3>> Group<TRecord, TKey1, TKey2, TKey3>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TKey1>> key1,
            Expression<Func<TRecord, TKey2>> key2,
            Expression<Func<TRecord, TKey3>> key3
            )
        {
            return new GroupByFunctionQuery<TRecord, TKey1, TKey2, TKey3, Tuple<TKey1, TKey2, TKey3>>(sequenceQuery, key1, key2, key3);
        }

        public static GroupByFunctionQuery<TRecord, TKey1, TKey2, TKey3, TGroupingKey> Group<TRecord, TKey1, TKey2, TKey3, TGroupingKey>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TKey1>> key1,
            Expression<Func<TRecord, TKey2>> key2,
            Expression<Func<TRecord, TKey3>> key3
            )
        {
            return new GroupByFunctionQuery<TRecord, TKey1, TKey2, TKey3, TGroupingKey>(sequenceQuery, key1, key2, key3);
        }

        public static UngroupQuery<TKey, TValue, UngroupObject<TKey, TValue>> Ungroup<TKey, TValue>(this IGroupingQuery<TKey, TValue> groupingQuery)
        {
            return new UngroupQuery<TKey, TValue, UngroupObject<TKey, TValue>>(groupingQuery);
        }

        public static UngroupQuery<TKey, TValue, TResult> Ungroup<TKey, TValue, TResult>(this IGroupingQuery<TKey, TValue> groupingQuery)
        {
            return new UngroupQuery<TKey, TValue, TResult>(groupingQuery);
        }

        public static MapGroupQuery<TKey, TOriginal, TTarget> Map<TKey, TOriginal, TTarget>(
            this IGroupingQuery<TKey, TOriginal[]> groupingQuery,
            Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            return new MapGroupQuery<TKey, TOriginal, TTarget>(groupingQuery, mapExpression);
        }

        public static ReduceGroupQuery<TKey, TRecord> Reduce<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, TRecord, TRecord>> reduceFunction)
        {
            return new ReduceGroupQuery<TKey, TRecord>(groupingQuery, reduceFunction);
        }

        public static MinGroupAggregateQuery<TKey, TRecord, TExpressionValue> Min<TKey, TRecord, TExpressionValue>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, TExpressionValue>> field = null
            )
        {
            return new MinGroupAggregateQuery<TKey, TRecord, TExpressionValue>(groupingQuery, field);
        }

        public static MinAggregateQuery<TRecord, TExpressionValue> Min<TRecord, TExpressionValue>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TExpressionValue>> field = null
            )
        {
            return new MinAggregateQuery<TRecord, TExpressionValue>(sequenceQuery, field);
        }

        public static MaxGroupAggregateQuery<TKey, TRecord, TExpressionValue> Max<TKey, TRecord, TExpressionValue>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, TExpressionValue>> field = null
            )
        {
            return new MaxGroupAggregateQuery<TKey, TRecord, TExpressionValue>(groupingQuery, field);
        }

        public static MaxAggregateQuery<TRecord, TExpressionValue> Max<TRecord, TExpressionValue>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TExpressionValue>> field = null
            )
        {
            return new MaxAggregateQuery<TRecord, TExpressionValue>(sequenceQuery, field);
        }

        public static AvgGroupAggregateQuery<TKey, TRecord, double> Avg<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, double>> field = null
            )
        {
            return new AvgGroupAggregateQuery<TKey, TRecord, double>(groupingQuery, field);
        }

        public static AvgGroupAggregateQuery<TKey, TRecord, TAvgType> Avg<TKey, TRecord, TAvgType>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, TAvgType>> field = null
            )
        {
            return new AvgGroupAggregateQuery<TKey, TRecord, TAvgType>(groupingQuery, field);
        }

        public static AvgAggregateQuery<TRecord, double> Avg<TRecord>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, double>> field = null
            )
        {
            return new AvgAggregateQuery<TRecord, double>(sequenceQuery, field);
        }

        public static AvgAggregateQuery<TRecord, TAvgType> Avg<TRecord, TAvgType>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TAvgType>> field = null
            )
        {
            return new AvgAggregateQuery<TRecord, TAvgType>(sequenceQuery, field);
        }

        public static SumGroupAggregateQuery<TKey, TRecord, double> Sum<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, double>> field = null
            )
        {
            return new SumGroupAggregateQuery<TKey, TRecord, double>(groupingQuery, field);
        }

        public static SumGroupAggregateQuery<TKey, TRecord, TSumType> Sum<TKey, TRecord, TSumType>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, TSumType>> field = null
            )
        {
            return new SumGroupAggregateQuery<TKey, TRecord, TSumType>(groupingQuery, field);
        }

        public static SumAggregateQuery<TRecord, double> Sum<TRecord>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, double>> field = null
            )
        {
            return new SumAggregateQuery<TRecord, double>(sequenceQuery, field);
        }

        public static SumAggregateQuery<TRecord, TSumType> Sum<TRecord, TSumType>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TSumType>> field = null
            )
        {
            return new SumAggregateQuery<TRecord, TSumType>(sequenceQuery, field);
        }

        public static CountGroupAggregateQuery<TKey, TRecord> Count<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, bool>> predicate = null
            )
        {
            return new CountGroupAggregateQuery<TKey, TRecord>(groupingQuery, predicate);
        }

        public static CountAggregateQuery<T> Count<T>(
            this ISequenceQuery<T> target,
            Expression<Func<T, bool>> predicate = null
            )
        {
            return new CountAggregateQuery<T>(target, predicate);
        }

        public static ContainsGroupAggregateQuery<TKey, TRecord>Contains<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, bool>> predicate = null
            )
        {
            return new ContainsGroupAggregateQuery<TKey, TRecord>(groupingQuery, predicate);
        }

        public static ContainsAggregateQuery<T> Contains<T>(
            this ISequenceQuery<T> target,
            Expression<Func<T, bool>> predicate = null
            )
        {
            return new ContainsAggregateQuery<T>(target, predicate);
        }

        #endregion
        #endregion
    }
}
