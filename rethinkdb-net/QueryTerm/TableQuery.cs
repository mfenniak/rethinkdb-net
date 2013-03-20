using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class TableQuery<T> : ISequenceQuery<T>
    {
        private readonly DbQuery dbTerm;
        private readonly string table;
        private readonly bool useOutdated;

        public TableQuery(DbQuery dbTerm, string table, bool useOutdated)
        {
            this.dbTerm = dbTerm;
            this.table = table;
            this.useOutdated = useOutdated;
        }

        public GetQuery<T> Get(string primaryKey, string primaryAttribute = null)
        {
            return new GetQuery<T>(this, primaryKey, primaryAttribute);
        }

        public InsertQuery<T> Insert(T @object, bool upsert = false)
        {
            return new InsertQuery<T>(this, new T[] { @object }, upsert);
        }

        public InsertQuery<T> Insert(T[] @objects, bool upsert = false)
        {
            return new InsertQuery<T>(this, @objects, upsert);
        }

        Spec.Term ISequenceQuery<T>.GenerateTerm()
        {
            var tableTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.TABLE,
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
            if (useOutdated)
            {
                tableTerm.optargs.Add(new Spec.Term.AssocPair()
                {
                    key = "use_outdated",
                    val = new Spec.Term()
                    {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_BOOL,
                            r_bool = useOutdated,
                        }
                    }
                });
            }
            return tableTerm;
        }
    }
}