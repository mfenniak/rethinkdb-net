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

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE_CREATE,
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

            if (datacenter != null)
            {
                tableTerm.optargs.Add(new Term.AssocPair()
                {
                    key = "datacenter",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = datacenter,
                        }
                    }
                });
            }

            if (primaryKey != null)
            {
                tableTerm.optargs.Add(new Term.AssocPair()
                {
                    key = "primary_key",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = primaryKey,
                        }
                    }
                });
            }

            if (cacheSize.HasValue)
            {
                tableTerm.optargs.Add(new Term.AssocPair()
                {
                    key = "cache_size",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = cacheSize.Value,
                        }
                    }
                });
            }

            return tableTerm;
        }
    }
}
