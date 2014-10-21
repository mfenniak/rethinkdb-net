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

        public static IDatabaseQuery Db(string db)
        {
            return new DbQuery(db);
        }

        public static IWriteQuery<DmlResponse> DbCreate(string db)
        {
            return new DbCreateQuery(db);
        }

        public static IWriteQuery<DmlResponse> DbDrop(string db)
        {
            return new DbDropQuery(db);
        }

        public static ISingleObjectQuery<string[]> DbList()
        {
            return new DbListQuery();
        }

        #endregion
        #region Table Operations

        public static ITableQuery<T> Table<T>(this IDatabaseQuery target, string table, bool useOutdated = false)
        {
            return new TableQuery<T>(target, table, useOutdated);
        }

        public static IWriteQuery<DmlResponse> TableCreate(this IDatabaseQuery target, string table, string datacenter = null, string primaryKey = null, double? cacheSize = null)
        {
            return new TableCreateQuery(target, table, datacenter, primaryKey, cacheSize);
        }

        public static IWriteQuery<DmlResponse> TableDrop(this IDatabaseQuery target, string table)
        {
            return new TableDropQuery(target, table);
        }

        public static ISingleObjectQuery<string[]> TableList(this IDatabaseQuery target)
        {
            return new TableListQuery(target);
        }

        public static IWriteQuery<DmlResponse> IndexCreate<T, TIndexExpression>(this ITableQuery<T> target, string indexName, Expression<Func<T, TIndexExpression>> indexExpression, bool multiIndex = false)
        {
            return new IndexCreateQuery<T, TIndexExpression>(target, indexName, indexExpression, multiIndex);
        }

        public static ISequenceQuery<string> IndexList<T>(this ITableQuery<T> target)
        {
            return new IndexListQuery<T>(target);
        }

        public static IWriteQuery<DmlResponse> IndexDrop<T>(this ITableQuery<T> target, string indexName)
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

        public static IWriteQuery<DmlResponse> IndexCreate<TRecord, TIndex>(this IIndex<TRecord, TIndex> index)
        {
            return index.Table.IndexCreate(index.Name, index.IndexAccessor, false);
        }

        public static IWriteQuery<DmlResponse> IndexCreate<TRecord, TIndex>(this IMultiIndex<TRecord, TIndex> index)
        {
            return index.Table.IndexCreate(index.Name, index.IndexAccessor, true);
        }

        public static IWriteQuery<DmlResponse> IndexDrop<TRecord, TIndex>(this IBaseIndex<TRecord, TIndex> index)
        {
            return index.Table.IndexDrop(index.Name);
        }

        public static IWriteQuery<DmlResponse> Insert<T>(this ITableQuery<T> target, T @object, bool upsert = false)
        {
            return new InsertQuery<T>(target, new T[] { @object }, upsert);
        }

        public static IWriteQuery<DmlResponse> Insert<T>(this ITableQuery<T> target, IEnumerable<T> @objects, bool upsert = false)
        {
            return new InsertQuery<T>(target, @objects, upsert);
        }

        #endregion
        #region Query Operations

        public static IMutableSingleObjectQuery<T> Get<T>(this ISequenceQuery<T> target, string primaryKey, string primaryAttribute = null)
        {
            return new GetQuery<T>(target, primaryKey, primaryAttribute);
        }

        public static IMutableSingleObjectQuery<T> Get<T>(this ISequenceQuery<T> target, double primaryKey, string primaryAttribute = null)
        {
            return new GetQuery<T>(target, primaryKey, primaryAttribute);
        }

        public static ISequenceQuery<TSequence> GetAll<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey key, string indexName = null)
        {
            return new GetAllQuery<TSequence, TKey>(target, new TKey[] { key }, indexName);
        }

        public static ISequenceQuery<TSequence> GetAll<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey[] keys, string indexName = null)
        {
            return new GetAllQuery<TSequence, TKey>(target, keys, indexName);
        }

        public static ISequenceQuery<TSequence> GetAll<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey key, IBaseIndex<TSequence, TKey> index)
        {
            return target.GetAll(key, indexName: index.Name);
        }

        public static ISequenceQuery<TSequence> GetAll<TSequence, TKey>(this IBaseIndex<TSequence, TKey> index, params TKey[] keys)
        {
            return index.Table.GetAll(keys: keys, indexName: index.Name);
        }

        public static ISequenceQuery<T> Filter<T>(this ISequenceQuery<T> target, Expression<Func<T, bool>> filterExpression)
        {
            return new FilterQuery<T>(target, filterExpression);
        }

        // LINQ-compatible alias for Filter
        public static ISequenceQuery<T> Where<T>(this ISequenceQuery<T> target, Expression<Func<T, bool>> filterExpression)
        {
            return target.Filter(filterExpression);
        }

        public static IWriteQuery<DmlResponse> Update<T>(this ISequenceQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateQuery<T>(target, updateExpression, nonAtomic);
        }

        public static IWriteQuery<DmlResponse> Update<T>(this IMutableSingleObjectQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateQuery<T>(target, updateExpression, nonAtomic);
        }

        public static IWriteQuery<DmlResponse<T>> UpdateAndReturnChanges<T>(this IMutableSingleObjectQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
        {
            return new UpdateAndReturnValueQuery<T>(target, updateExpression, nonAtomic);
        }

        public static IWriteQuery<DmlResponse> Delete<T>(this ISequenceQuery<T> target)
        {
            return new DeleteQuery<T>(target);
        }

        public static IWriteQuery<DmlResponse> Delete<T>(this IMutableSingleObjectQuery<T> target)
        {
            return new DeleteQuery<T>(target);
        }

        public static IWriteQuery<DmlResponse<T>> DeleteAndReturnChanges<T>(this IMutableSingleObjectQuery<T> target)
        {
            return new DeleteAndReturnValueQuery<T>(target);
        }

        public static IWriteQuery<DmlResponse> Replace<T>(this IMutableSingleObjectQuery<T> target, T newObject, bool nonAtomic = false)
        {
            return new ReplaceQuery<T>(target, newObject, nonAtomic);
        }

        public static IWriteQuery<DmlResponse<T>> ReplaceAndReturnChanges<T>(this IMutableSingleObjectQuery<T> target, T newObject, bool nonAtomic = false)
        {
            return new ReplaceAndReturnValueQuery<T>(target, newObject, nonAtomic);
        }

        public static ISequenceQuery<TSequence> Between<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey leftKey, TKey rightKey, string indexName = null, Bound leftBound = Bound.Closed, Bound rightBound = Bound.Open)
        {
            return new BetweenQuery<TSequence, TKey>(target, leftKey, rightKey, indexName, leftBound, rightBound);
        }

        public static ISequenceQuery<TSequence> Between<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey leftKey, TKey rightKey, IBaseIndex<TSequence, TKey> index, Bound leftBound = Bound.Closed, Bound rightBound = Bound.Open)
        {
            return target.Between(leftKey, rightKey, index.Name, leftBound, rightBound);
        }

        public static ISingleObjectQuery<T> Expr<T>(T @object)
        {
            return new ExprQuery<T>(@object);
        }

        public static ISingleObjectQuery<T> Expr<T>(Expression<Func<T>> objectExpr)
        {
            return new ExprQuery<T>(objectExpr);
        }

        public static ISequenceQuery<T> Expr<T>(IEnumerable<T> enumerable)
        {
            return new ExprSequenceQuery<T>(enumerable);
        }

        public static ISequenceQuery<TTarget> Map<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            return new MapQuery<TOriginal, TTarget>(sequenceQuery, mapExpression);
        }

        // LINQ-compatible alias for Map
        public static ISequenceQuery<TTarget> Select<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            return sequenceQuery.Map(mapExpression);
        }

        public static IOrderedSequenceQuery<T> OrderBy<T>(
            this ISequenceQuery<T> sequenceQuery,
            Expression<Func<T, object>> memberReferenceExpression,
            OrderByDirection direction = OrderByDirection.Ascending)
        {
            return new OrderByQuery<T>(sequenceQuery, new OrderByTerm<T> {
                Expression = memberReferenceExpression,
                Direction = direction
            });
        }

        public static IOrderedSequenceQuery<T> OrderBy<T>(
            this ISequenceQuery<T> sequenceQuery,
            string indexName,
            OrderByDirection direction = OrderByDirection.Ascending)
        {
            return new OrderByQuery<T>(sequenceQuery, new OrderByTerm<T> {
                Direction = direction,
                IndexName = indexName,
            });
        }

        public static IOrderedSequenceQuery<T> OrderBy<T, TIndexType>(
            this ISequenceQuery<T> sequenceQuery,
            IIndex<T, TIndexType> index,
            OrderByDirection direction = OrderByDirection.Ascending)
        {
            return new OrderByQuery<T>(sequenceQuery, new OrderByTerm<T> {
                Direction = direction,
                IndexName = index.Name,
            });
        }

        // LINQ-compatible alias for OrderBy
        public static IOrderedSequenceQuery<T> OrderByDescending<T>(
            this ISequenceQuery<T> sequenceQuery,
            Expression<Func<T, object>> memberReferenceExpression)
        {
            return sequenceQuery.OrderBy(memberReferenceExpression, OrderByDirection.Descending);
        }
        public static IOrderedSequenceQuery<T> OrderByDescending<T>(
            this ISequenceQuery<T> sequenceQuery,
            string indexName)
        {
            return sequenceQuery.OrderBy(indexName, OrderByDirection.Descending);
        }
        public static IOrderedSequenceQuery<T> OrderByDescending<T, TIndexType>(
            this ISequenceQuery<T> sequenceQuery,
            IIndex<T, TIndexType> index)
        {
            return sequenceQuery.OrderBy(index, OrderByDirection.Descending);
        }

        public static IOrderedSequenceQuery<T> ThenBy<T>(
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
        public static IOrderedSequenceQuery<T> ThenByDescending<T>(
            this IOrderedSequenceQuery<T> orderByQuery,
            Expression<Func<T, object>> memberReferenceExpression)
        {
            return orderByQuery.ThenBy(memberReferenceExpression, OrderByDirection.Descending);
        }

        public static ISequenceQuery<T> Skip<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return new SkipQuery<T>(sequenceQuery, count);
        }

        public static ISequenceQuery<T> Limit<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return new LimitQuery<T>(sequenceQuery, count);
        }

        // LINQ compatible alias for Limit
        public static ISequenceQuery<T> Take<T>(this ISequenceQuery<T> sequenceQuery, int count)
        {
            return sequenceQuery.Limit(count);
        }

        public static ISequenceQuery<T> Slice<T>(this ISequenceQuery<T> sequenceQuery, int startIndex, int? endIndex = null)
        {
            return new SliceQuery<T>(sequenceQuery, startIndex, endIndex);
        }

        public static ISequenceQuery<Tuple<TLeft, TRight>> InnerJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new InnerJoinQuery<TLeft, TRight>(leftQuery, rightQuery, joinPredicate);
        }

        public static ISequenceQuery<Tuple<TLeft, TRight>> OuterJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, ISequenceQuery<TRight> rightQuery, Expression<Func<TLeft, TRight, bool>> joinPredicate)
        {
            return new OuterJoinQuery<TLeft, TRight>(leftQuery, rightQuery, joinPredicate);
        }

        public static ISequenceQuery<TTarget> Zip<TLeft, TRight, TTarget>(this ISequenceQuery<Tuple<TLeft, TRight>> sequenceQuery)
        {
            return new ZipQuery<TLeft, TRight, TTarget>(sequenceQuery);
        }

        public static ISequenceQuery<Tuple<TLeft, TRight>> EqJoin<TLeft, TRight>(
            this ISequenceQuery<TLeft> leftQuery,
            Expression<Func<TLeft, object>> leftMemberReferenceExpression,
            ISequenceQuery<TRight> rightQuery,
            string indexName = null)
        {
            return new EqJoinQuery<TLeft, TRight>(leftQuery, leftMemberReferenceExpression, rightQuery, indexName);
        }

        public static ISequenceQuery<Tuple<TLeft, TRight>> EqJoin<TLeft, TRight, TIndexType>(
            this ISequenceQuery<TLeft> leftQuery,
            Expression<Func<TLeft, object>> leftMemberReferenceExpression,
            ISequenceQuery<TRight> rightQuery,
            IBaseIndex<TRight, TIndexType> index)
        {
            return leftQuery.EqJoin(leftMemberReferenceExpression, rightQuery, index.Name);
        }

        public static ISingleObjectQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction);
        }

        public static ISingleObjectQuery<T> Nth<T>(this ISequenceQuery<T> sequenceQuery, int index)
        {
            return new NthQuery<T>(sequenceQuery, index);
        }

        public static ISequenceQuery<T> Distinct<T>(this ISequenceQuery<T> sequenceQuery)
        {
            return new DistinctQuery<T>(sequenceQuery);
        }

        public static ISequenceQuery<TTarget> ConcatMap<TOriginal, TTarget>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, IEnumerable<TTarget>>> mapping)
        {
            return new ConcatMapQuery<TOriginal, TTarget>(sequenceQuery, mapping);
        }

        public static ISequenceQuery<T> Union<T>(this ISequenceQuery<T> query1, ISequenceQuery<T> query2)
        {
            return new UnionQuery<T>(query1, query2);
        }

        public static ISingleObjectQuery<DateTimeOffset> Now()
        {
            return new NowQuery();
        }

        public static ISequenceQuery<T> Sample<T>(this ISequenceQuery<T> target, int count)
        {
            return new SampleQuery<T>(target, count);
        }

        public static ISequenceQuery<T> HasFields<T>(this ISequenceQuery<T> target, params Expression<Func<T, object>>[] fields)
        {
            return new HasFieldsSequenceQuery<T>(target, fields);
        }

        public static ISingleObjectQuery<bool> HasFields<T>(this ISingleObjectQuery<T> target, params Expression<Func<T, object>>[] fields)
        {
            return new HasFieldsSingleObjectQuery<T>(target, fields);
        }

        #region Grouping and Aggregation

        public static IGroupingQuery<TIndexType, TRecord[]> Group<TRecord, TIndexType>(
            // Can only use indexName on Group on a TABLE, not any arbitrary sequence
            this ITableQuery<TRecord> table,
            string indexName
            )
        {
            return new GroupByIndexQuery<TRecord, TIndexType>(table, indexName);
        }

        public static IGroupingQuery<TIndexType, TRecord[]> Group<TRecord, TIndexType>(
            // Can only use indexName on Group on a TABLE, not any arbitrary sequence
            this ITableQuery<TRecord> table,
            IIndex<TRecord, TIndexType> index
            )
        {
            return table.Group<TRecord, TIndexType>(index.Name);
        }

        public static IGroupingQuery<TKey, TRecord[]> Group<TRecord, TKey>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TKey>> key
            )
        {
            return new GroupByFunctionQuery<TRecord, TKey>(sequenceQuery, key);
        }

        public static IGroupingQuery<Tuple<TKey1, TKey2>, TRecord[]> Group<TRecord, TKey1, TKey2>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TKey1>> key1,
            Expression<Func<TRecord, TKey2>> key2
            )
        {
            return new GroupByFunctionQuery<TRecord, TKey1, TKey2>(sequenceQuery, key1, key2);
        }

        public static IGroupingQuery<Tuple<TKey1, TKey2, TKey3>, TRecord[]> Group<TRecord, TKey1, TKey2, TKey3>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TKey1>> key1,
            Expression<Func<TRecord, TKey2>> key2,
            Expression<Func<TRecord, TKey3>> key3
            )
        {
            return new GroupByFunctionQuery<TRecord, TKey1, TKey2, TKey3>(sequenceQuery, key1, key2, key3);
        }

        public static ISequenceQuery<UngroupObject<TKey, TValue>> Ungroup<TKey, TValue>(this IGroupingQuery<TKey, TValue> groupingQuery)
        {
            return new UngroupQuery<TKey, TValue>(groupingQuery);
        }

        public static IGroupingQuery<TKey, TTarget[]> Map<TKey, TOriginal, TTarget>(
            this IGroupingQuery<TKey, TOriginal[]> groupingQuery,
            Expression<Func<TOriginal, TTarget>> mapExpression)
        {
            return new MapGroupQuery<TKey, TOriginal, TTarget>(groupingQuery, mapExpression);
        }

        public static IGroupingQuery<TKey, TRecord> Reduce<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, TRecord, TRecord>> reduceFunction)
        {
            return new ReduceGroupQuery<TKey, TRecord>(groupingQuery, reduceFunction);
        }

        public static IGroupingQuery<TKey, TRecord> Min<TKey, TRecord, TExpressionValue>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, TExpressionValue>> field = null
            )
        {
            return new MinGroupAggregateQuery<TKey, TRecord, TExpressionValue>(groupingQuery, field);
        }

        public static ISingleObjectQuery<TRecord> Min<TRecord, TExpressionValue>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TExpressionValue>> field = null
            )
        {
            return new MinAggregateQuery<TRecord, TExpressionValue>(sequenceQuery, field);
        }

        public static IGroupingQuery<TKey, TRecord> Max<TKey, TRecord, TExpressionValue>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, TExpressionValue>> field = null
            )
        {
            return new MaxGroupAggregateQuery<TKey, TRecord, TExpressionValue>(groupingQuery, field);
        }

        public static ISingleObjectQuery<TRecord> Max<TRecord, TExpressionValue>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, TExpressionValue>> field = null
            )
        {
            return new MaxAggregateQuery<TRecord, TExpressionValue>(sequenceQuery, field);
        }

        public static IGroupingQuery<TKey, double> Avg<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, double>> field = null
            )
        {
            return new AvgGroupAggregateQuery<TKey, TRecord>(groupingQuery, field);
        }

        public static ISingleObjectQuery<double> Avg<TRecord>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, double>> field = null
            )
        {
            return new AvgAggregateQuery<TRecord>(sequenceQuery, field);
        }

        public static IGroupingQuery<TKey, double> Sum<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, double>> field = null
            )
        {
            return new SumGroupAggregateQuery<TKey, TRecord>(groupingQuery, field);
        }

        public static ISingleObjectQuery<double> Sum<TRecord>(
            this ISequenceQuery<TRecord> sequenceQuery,
            Expression<Func<TRecord, double>> field = null
            )
        {
            return new SumAggregateQuery<TRecord>(sequenceQuery, field);
        }

        public static IGroupingQuery<TKey, int> Count<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, bool>> predicate = null
            )
        {
            return new CountGroupAggregateQuery<TKey, TRecord>(groupingQuery, predicate);
        }

        public static ISingleObjectQuery<int> Count<T>(
            this ISequenceQuery<T> target,
            Expression<Func<T, bool>> predicate = null
            )
        {
            return new CountAggregateQuery<T>(target, predicate);
        }

        public static IGroupingQuery<TKey, bool> Contains<TKey, TRecord>(
            this IGroupingQuery<TKey, TRecord[]> groupingQuery,
            Expression<Func<TRecord, bool>> predicate = null
            )
        {
            return new ContainsGroupAggregateQuery<TKey, TRecord>(groupingQuery, predicate);
        }

        public static ISingleObjectQuery<bool> Contains<T>(
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
