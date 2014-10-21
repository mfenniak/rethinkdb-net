using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class GroupByIndexQuery<TRecord, TIndexType> : IGroupingQuery<TIndexType, TRecord[]>
    {
        private ITableQuery<TRecord> tableQuery;
        private string indexName;

        public GroupByIndexQuery(ITableQuery<TRecord> tableQuery, string indexName)
        {
            this.tableQuery = tableQuery;
            this.indexName = indexName;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var term = new Term()
            {
                type = Term.TermType.GROUP,
            };
            term.args.Add(tableQuery.GenerateTerm(datumConverterFactory));
            term.args.Add(
                new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = indexName,
                    }
                }
            );
            return term;
        }
    }
}
