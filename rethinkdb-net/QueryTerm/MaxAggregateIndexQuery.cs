using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class MaxAggregateIndexQuery<TRecord, TIndexType> : ISingleObjectQuery<TRecord>, IChangefeedCompatibleQuery<TRecord>
    {
        private readonly ITableQuery<TRecord> tableQuery;
        private readonly string indexName;

        public MaxAggregateIndexQuery(ITableQuery<TRecord> tableQuery, string indexName)
        {
            this.tableQuery = tableQuery;
            this.indexName = indexName;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.MAX,
            };
            term.args.Add(tableQuery.GenerateTerm(queryConverter));
            term.optargs.Add(
                new Term.AssocPair()
                {
                    key = "index",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = indexName
                        }
                    }
                }
            );
            return term;
        }
    }
}
