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

        Spec.Term ISingleObjectQuery<DmlResponse>.GenerateTerm()
        {
            var tableTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.TABLE_DROP,
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
