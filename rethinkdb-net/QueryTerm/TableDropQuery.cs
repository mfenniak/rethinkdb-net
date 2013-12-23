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

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE_DROP,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm(datumConverterFactory, expressionConverterFactory));
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
