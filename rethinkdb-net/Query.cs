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
            return new GetAllQuery<TSequence, TKey>(target, key, indexName);
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

        public static IWriteQuery<DmlResponse<T>> UpdateAndReturnValue<T>(this IMutableSingleObjectQuery<T> target, Expression<Func<T, T>> updateExpression, bool nonAtomic = false)
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

        public static IWriteQuery<DmlResponse<T>> DeleteAndReturnValue<T>(this IMutableSingleObjectQuery<T> target)
        {
            return new DeleteAndReturnValueQuery<T>(target);
        }

        public static IWriteQuery<DmlResponse> Replace<T>(this IMutableSingleObjectQuery<T> target, T newObject, bool nonAtomic = false)
        {
            return new ReplaceQuery<T>(target, newObject, nonAtomic);
        }

        public static IWriteQuery<DmlResponse<T>> ReplaceAndReturnValue<T>(this IMutableSingleObjectQuery<T> target, T newObject, bool nonAtomic = false)
        {
            return new ReplaceAndReturnValueQuery<T>(target, newObject, nonAtomic);
        }

        public static ISequenceQuery<TSequence> Between<TSequence, TKey>(this ISequenceQuery<TSequence> target, TKey leftKey, TKey rightKey, string indexName = null, Bound leftBound = Bound.Closed, Bound rightBound = Bound.Open)
        {
            return new BetweenQuery<TSequence, TKey>(target, leftKey, rightKey, indexName, leftBound, rightBound);
        }

        public static ISingleObjectQuery<double> Count<T>(this ISequenceQuery<T> target)
        {
            return new CountQuery<T>(target);
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

        public static IOrderedSequenceQuery<T> OrderBy<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, object>> memberReferenceExpression, OrderByDirection direction = OrderByDirection.Ascending, string indexName = null)
        {
            return new OrderByQuery<T>(sequenceQuery, new OrderByTerm<T> {
                Expression = memberReferenceExpression,
                Direction = direction,
                IndexName = indexName
            });
        }

        // LINQ-compatible alias for OrderBy
        public static IOrderedSequenceQuery<T> OrderByDescending<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, object>> memberReferenceExpression, string indexName = null)
        {
            return sequenceQuery.OrderBy(memberReferenceExpression, OrderByDirection.Descending, indexName);
        }

        public static IOrderedSequenceQuery<T> ThenBy<T>(this IOrderedSequenceQuery<T> orderByQuery, Expression<Func<T, object>> memberReferenceExpression, OrderByDirection direction = OrderByDirection.Ascending)
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
        public static IOrderedSequenceQuery<T> ThenByDescending<T>(this IOrderedSequenceQuery<T> orderByQuery, Expression<Func<T, object>> memberReferenceExpression)
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

        public static ISequenceQuery<Tuple<TLeft, TRight>> EqJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, Expression<Func<TLeft, object>> leftMemberReferenceExpression, ISequenceQuery<TRight> rightQuery)
        {
            return new EqJoinQuery<TLeft, TRight>(leftQuery, leftMemberReferenceExpression, rightQuery, null);
        }

        public static ISequenceQuery<Tuple<TLeft, TRight>> EqJoin<TLeft, TRight>(this ISequenceQuery<TLeft> leftQuery, Expression<Func<TLeft, object>> leftMemberReferenceExpression, ISequenceQuery<TRight> rightQuery, string indexName)
        {
            return new EqJoinQuery<TLeft, TRight>(leftQuery, leftMemberReferenceExpression, rightQuery, indexName);
        }

        public static ISingleObjectQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction);
        }

        public static ISingleObjectQuery<T> Reduce<T>(this ISequenceQuery<T> sequenceQuery, Expression<Func<T, T, T>> reduceFunction, T @base)
        {
            return new ReduceQuery<T>(sequenceQuery, reduceFunction, @base);
        }

        public static ISingleObjectQuery<T> Nth<T>(this ISequenceQuery<T> sequenceQuery, int index)
        {
            return new NthQuery<T>(sequenceQuery, index);
        }

        public static ISequenceQuery<T> Distinct<T>(this ISequenceQuery<T> sequenceQuery)
        {
            return new DistinctQuery<T>(sequenceQuery);
        }

#if false
        public static ISequenceQuery<Tuple<TGroup, TMap>> GroupedMapReduce<TOriginal, TGroup, TMap>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TGroup>> grouping, Expression<Func<TOriginal, TMap>> mapping, Expression<Func<TMap, TMap, TMap>> reduction)
        {
            return new GroupedMapReduceQuery<TOriginal, TGroup, TMap>(sequenceQuery, grouping, mapping, reduction);
        }

        public static ISequenceQuery<Tuple<TGroup, TMap>> GroupedMapReduce<TOriginal, TGroup, TMap>(this ISequenceQuery<TOriginal> sequenceQuery, Expression<Func<TOriginal, TGroup>> grouping, Expression<Func<TOriginal, TMap>> mapping, Expression<Func<TMap, TMap, TMap>> reduction, TMap @base)
        {
            return new GroupedMapReduceQuery<TOriginal, TGroup, TMap>(sequenceQuery, grouping, mapping, reduction, @base);
        }
#endif

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

#if false
        public static ISequenceQuery<Tuple<TGroupKeyType, TReductionType>> GroupBy<TObject, TReductionType, TGroupKeyType>(this ISequenceQuery<TObject> sequenceQuery, IGroupByReduction<TReductionType> reductionObject, Expression<Func<TObject, TGroupKeyType>> groupKeyConstructor)
        {
            return new GroupByQuery<TObject, TReductionType, TGroupKeyType>(sequenceQuery, reductionObject, groupKeyConstructor);
        }
#endif

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

        #endregion
        #region Prebuilt GroupBy reductions

        public static IGroupByReduction<double> Count()
        {
            return CountReduction.Instance;
        }

        public static IGroupByReduction<double> Sum<TObject>(Expression<Func<TObject, double>> numericMemberReference)
        {
            return new SumReduction<TObject>(numericMemberReference);
        }

        public static IGroupByReduction<double> Avg<TObject>(Expression<Func<TObject, double>> numericMemberReference)
        {
            return new AvgReduction<TObject>(numericMemberReference);
        }

        #endregion
    }
}
