using System;
using RethinkDb.DatumConverters;
using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class BetweenQuery<TSequence, TKey> : ISequenceQuery<TSequence>
    {
        private readonly ISequenceQuery<TSequence> tableTerm;
        private readonly TKey leftKey;
        private readonly TKey rightKey;
        private readonly string indexName;
        private readonly Bound leftBound;
        private readonly Bound rightBound;

        public BetweenQuery(ISequenceQuery<TSequence> tableTerm, TKey leftKey, TKey rightKey, string indexName, Bound leftBound, Bound rightBound)
        {
            this.tableTerm = tableTerm;
            this.leftKey = leftKey;
            this.rightKey = rightKey;
            this.indexName = indexName;
            this.leftBound = leftBound;
            this.rightBound = rightBound;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var datumConverter = datumConverterFactory.Get<TKey>();
            var betweenTerm = new Term()
            {
                type = Term.TermType.BETWEEN,
            };
            betweenTerm.args.Add(tableTerm.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            betweenTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = datumConverter.ConvertObject(leftKey)
            });
            betweenTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = datumConverter.ConvertObject(rightKey)
            });
            if (!String.IsNullOrEmpty(indexName))
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
            if (leftBound != Bound.Closed)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "left_bound",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_STR,
                            r_str = "open"
                        },
                    }
                });
            }
            if (rightBound != Bound.Open)
            {
                 betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "right_bound",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum() {
                            type = Datum.DatumType.R_STR,
                            r_str = "closed"
                        },
                    }
                });
           }
           return betweenTerm;
        }
    }
}