using System;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class TableCreateQuery : IWriteQuery<DmlResponse>
    {
        private readonly IDatabaseQuery dbTerm;
        private readonly string table;
        private readonly string datacenter;
        private readonly string primaryKey;
        private readonly double? cacheSize;

        public TableCreateQuery(IDatabaseQuery dbTerm, string table, string datacenter, string primaryKey, double? cacheSize)
        {
            this.dbTerm = dbTerm;
            this.table = table;
            this.datacenter = datacenter;
            this.primaryKey = primaryKey;
            this.cacheSize = cacheSize;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE_CREATE,
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

            if (!String.IsNullOrEmpty(datacenter))
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

            if (!String.IsNullOrEmpty(primaryKey))
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
