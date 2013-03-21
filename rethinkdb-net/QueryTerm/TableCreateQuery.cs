using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class TableCreateQuery : IDmlQuery
    {
        private readonly DbQuery dbTerm;
        private readonly string table;
        private readonly string datacenter;
        private readonly string primaryKey;
        private readonly double? cacheSize;

        public TableCreateQuery(DbQuery dbTerm, string table, string datacenter, string primaryKey, double? cacheSize)
        {
            this.dbTerm = dbTerm;
            this.table = table;
            this.datacenter = datacenter;
            this.primaryKey = primaryKey;
            this.cacheSize = cacheSize;
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

            if (datacenter != null)
            {
                tableTerm.optargs.Add(new Spec.Term.AssocPair()
                {
                    key = "datacenter",
                    val = new Spec.Term()
                    {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_STR,
                            r_str = datacenter,
                        }
                    }
                });
            }

            if (primaryKey != null)
            {
                tableTerm.optargs.Add(new Spec.Term.AssocPair()
                {
                    key = "primary_key",
                    val = new Spec.Term()
                    {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_STR,
                            r_str = primaryKey,
                        }
                    }
                });
            }

            if (cacheSize.HasValue)
            {
                tableTerm.optargs.Add(new Spec.Term.AssocPair()
                {
                    key = "cache_size",
                    val = new Spec.Term()
                    {
                        type = Spec.Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_NUM,
                            r_num = cacheSize.Value,
                        }
                    }
                });
            }

            return tableTerm;
        }
    }
}
