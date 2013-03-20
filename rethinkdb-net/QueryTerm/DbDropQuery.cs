using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DbDropQuery : IDmlQuery
    {
        private readonly string db;

        public DbDropQuery(string db)
        {
            this.db = db;
        }

        Spec.Term ISingleObjectQuery<DmlResponse>.GenerateTerm()
        {
            var dbTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.DB_DROP,
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
