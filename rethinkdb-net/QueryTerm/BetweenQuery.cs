using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class BetweenQuery<TSequence, TKey> : ISequenceQuery<TSequence>
    {
        private readonly ISequenceQuery<TSequence> tableTerm;
        private readonly TKey leftKey;
        private readonly TKey rightKey;
        private readonly string indexName;

        public BetweenQuery(ISequenceQuery<TSequence> tableTerm, TKey leftKey, TKey rightKey, string indexName)
        {
            this.tableTerm = tableTerm;
            this.leftKey = leftKey;
            this.rightKey = rightKey;
            this.indexName = indexName;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var datumConverter = datumConverterFactory.Get<TKey>();
            var betweenTerm = new Term()
            {
                type = Term.TermType.BETWEEN,
            };
            betweenTerm.args.Add(tableTerm.GenerateTerm(datumConverterFactory));
            betweenTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = datumConverter.ConvertObject(leftKey)
            });
            betweenTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = datumConverter.ConvertObject(rightKey)
            });
            if (indexName != null)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
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
            return betweenTerm;
        }
    }
}