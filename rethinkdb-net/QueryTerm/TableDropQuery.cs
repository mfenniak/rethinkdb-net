using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class TableDropQuery : IWriteQuery<DmlResponse>
    {
        private readonly IDatabaseQuery dbTerm;
        private readonly string table;

        public TableDropQuery(IDatabaseQuery dbTerm, string table)
        {
            this.dbTerm = dbTerm;
            this.table = table;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE_DROP,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm(queryConverter));
            tableTerm.args.Add(
                new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = table,
                    }
                }
            );
            return tableTerm;
        }
    }
}
