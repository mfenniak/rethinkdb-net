using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class IndexWaitQuery<TTable> : ISequenceQuery<IndexStatus>
    {
        private readonly ITableQuery<TTable> tableTerm;
        private readonly string[] indexNames;

        public IndexWaitQuery(ITableQuery<TTable> tableTerm, string[] indexNames)
        {
            this.tableTerm = tableTerm;
            this.indexNames = indexNames;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.INDEX_WAIT,
            };
            term.args.Add(tableTerm.GenerateTerm(queryConverter));
            foreach (var indexName in indexNames)
            {
                term.args.Add(new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = indexName,
                    }
                });
            }
            return term;
        }
    }
}
