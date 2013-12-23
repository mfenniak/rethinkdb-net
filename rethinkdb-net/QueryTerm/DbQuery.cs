using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DbQuery : IDatabaseQuery
    {
        private readonly string db;

        public DbQuery(string db)
        {
            this.db = db;
        }

        public TableCreateQuery TableCreate(string table, string datacenter = null, string primaryKey = null, double? cacheSize = null)
        {
            return new TableCreateQuery(this, table, datacenter, primaryKey, cacheSize);
        }

        public TableDropQuery TableDrop(string table)
        {
            return new TableDropQuery(this, table);
        }

        public TableListQuery TableList()
        {
            return new TableListQuery(this);
        }

        public TableQuery<T> Table<T>(string table, bool useOutdated = false)
        {
            return new TableQuery<T>(this, table, useOutdated);
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var dbTerm = new Term()
            {
                type = Term.TermType.DB,
            };
            dbTerm.args.Add(
                new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_STR,
                        r_str = db,
                    }
                }
            );
            return dbTerm;
        }
    }
}
