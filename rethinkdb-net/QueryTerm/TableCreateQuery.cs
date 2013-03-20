using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class TableCreateQuery : IDmlQuery
    {
        private readonly DbQuery dbTerm;
        private readonly string table;

        public TableCreateQuery(DbQuery dbTerm, string table)
        {
            this.dbTerm = dbTerm;
            this.table = table;
        }

        Spec.Term ISingleObjectQuery<DmlResponse>.GenerateTerm()
        {
            var tableTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.TABLE_CREATE,
            };
            tableTerm.args.Add(dbTerm.GenerateTerm());
            tableTerm.args.Add(
                new Spec.Term()
                {
                    type = Spec.Term.TermType.DATUM,
                    datum = new Spec.Datum()
                    {
                        type = Spec.Datum.DatumType.R_STR,
                        r_str = table,
                    }
                }
            );
            return tableTerm;
        }
    }
}
