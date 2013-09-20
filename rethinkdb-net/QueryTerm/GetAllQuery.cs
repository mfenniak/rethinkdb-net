using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class GetAllQuery<TSequence, TKey> : ISequenceQuery<TSequence>
    {
        private readonly ISequenceQuery<TSequence> tableTerm;
        private readonly TKey key;
        private readonly string indexName;

        public GetAllQuery(ISequenceQuery<TSequence> tableTerm, TKey key, string indexName)
        {
            this.tableTerm = tableTerm;
            this.key = key;
            this.indexName = indexName;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var getAllTerm = new Term() {
                type = Term.TermType.GET_ALL,
            };
            getAllTerm.args.Add(tableTerm.GenerateTerm(datumConverterFactory));
            getAllTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = datumConverterFactory.Get<TKey>().ConvertObject(key)
            });
            if (!String.IsNullOrEmpty(indexName))
            {
                getAllTerm.optargs.Add(new Term.AssocPair() {
                    key = "index",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_STR,
                            r_str = indexName
                        },
                    }
                });
            }
            return getAllTerm;
        }
    }
}
