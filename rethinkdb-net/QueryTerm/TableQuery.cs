using RethinkDb.Spec;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class TableQuery<T> : ITableQuery<T>
    {
        private readonly IDatabaseQuery dbTerm;
        private readonly string table;
        private readonly bool useOutdated;

        public TableQuery(IDatabaseQuery dbTerm, string table, bool useOutdated)
        {
            this.dbTerm = dbTerm;
            this.table = table;
            this.useOutdated = useOutdated;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var tableTerm = new Term()
            {
                type = Term.TermType.TABLE,
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
            if (useOutdated)
            {
                tableTerm.optargs.Add(new Term.AssocPair()
                {
                    key = "use_outdated",
                    val = new Term()
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_BOOL,
                            r_bool = useOutdated,
                        }
                    }
                });
            }
            return tableTerm;
        }
    }
}