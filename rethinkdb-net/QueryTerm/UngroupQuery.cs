using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class UngroupQuery<TKey, TValue, TResult> : ISequenceQuery<TResult>
    {
        private readonly IGroupingQuery<TKey, TValue> groupingQuery;

        public UngroupQuery(IGroupingQuery<TKey, TValue> groupingQuery)
        {
            this.groupingQuery = groupingQuery;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.UNGROUP,
            };
            term.args.Add(groupingQuery.GenerateTerm(queryConverter));
            return term;
        }
    }
}
