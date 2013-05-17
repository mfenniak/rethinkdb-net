using RethinkDb.Spec;
using System.Linq.Expressions;
using System;

namespace RethinkDb.QueryTerm
{
    public class IndexDropQuery<TTable> : IDmlQuery
    {
        private readonly TableQuery<TTable> tableTerm;
        private readonly string indexName;

        public IndexDropQuery(TableQuery<TTable> tableTerm, string indexName)
        {
            this.tableTerm = tableTerm;
            this.indexName = indexName;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var indexDrop = new Term()
            {
                type = Term.TermType.INDEX_DROP,
            };
            indexDrop.args.Add(tableTerm.GenerateTerm(datumConverterFactory));
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

