using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class TableDropQuery : IDmlQuery
    {
        private readonly DbQuery dbTerm;
        private readonly string table;

        public TableDropQuery(DbQuery dbTerm, string table)
        {
            this.dbTerm = dbTerm;
            this.table = table;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE_DROP,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm(datumConverterFactory));
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
