using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DbDropQuery : IWriteQuery<DmlResponse>
    {
        private readonly string db;

        public DbDropQuery(string db)
        {
            this.db = db;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var dbTerm = new Term()
            {
                type = Term.TermType.DB_DROP,
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
