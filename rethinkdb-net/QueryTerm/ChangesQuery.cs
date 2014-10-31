using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class ChangesQuery<TRecord> : ISequenceQuery<DmlResponseChange<TRecord>>
    {
        private readonly ITableQuery<TRecord> tableQuery;

        public ChangesQuery(ITableQuery<TRecord> tableQuery)
        {
            this.tableQuery = tableQuery;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.CHANGES,
            };
            term.args.Add(tableQuery.GenerateTerm(queryConverter));
            return term;
        }
    }
}
