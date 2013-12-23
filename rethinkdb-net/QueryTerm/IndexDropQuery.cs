using RethinkDb.Spec;
using System.Linq.Expressions;
using System;

namespace RethinkDb.QueryTerm
{
    public class IndexDropQuery<TTable> : IWriteQuery<DmlResponse>
    {
        private readonly ITableQuery<TTable> tableTerm;
        private readonly string indexName;

        public IndexDropQuery(ITableQuery<TTable> tableTerm, string indexName)
        {
            this.tableTerm = tableTerm;
            this.indexName = indexName;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var indexDrop = new Term()
            {
                type = Term.TermType.INDEX_DROP,
            };
            indexDrop.args.Add(tableTerm.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            indexDrop.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = new Datum() {
                    type = Datum.DatumType.R_STR,
                    r_str = indexName
                },
            });
            return indexDrop;
        }
    }
}

