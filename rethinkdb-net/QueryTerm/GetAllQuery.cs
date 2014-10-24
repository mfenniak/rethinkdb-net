using System;
using System.Linq.Expressions;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class GetAllQuery<TSequence, TKey> : ISequenceQuery<TSequence>
    {
        private readonly ISequenceQuery<TSequence> tableTerm;
        private readonly TKey[] keys;
        private readonly string indexName;

        public GetAllQuery(ISequenceQuery<TSequence> tableTerm, TKey[] keys, string indexName)
        {
            this.tableTerm = tableTerm;
            this.keys = keys;
            this.indexName = indexName;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var getAllTerm = new Term() {
                type = Term.TermType.GET_ALL,
            };
            getAllTerm.args.Add(tableTerm.GenerateTerm(queryConverter));
            foreach (var key in keys)
            {
                getAllTerm.args.Add(new Term() {
                    type = Term.TermType.DATUM,
                    datum = queryConverter.Get<TKey>().ConvertObject(key)
                });
            }
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
