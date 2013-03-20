using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DbQuery
    {
        private readonly string db;

        public DbQuery(string db)
        {
            this.db = db;
        }

        public TableCreateQuery TableCreate(string table)
        {
            return new TableCreateQuery(this, table);
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

        public Spec.Term GenerateTerm()
        {
            var dbTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.DB,
            };
            dbTerm.args.Add(
                new Spec.Term()
                {
                    type = Spec.Term.TermType.DATUM,
                    datum = new Spec.Datum()
                    {
                        type = Spec.Datum.DatumType.R_STR,
                        r_str = db,
                    }
                }
            );
            return dbTerm;
        }
    }
}
