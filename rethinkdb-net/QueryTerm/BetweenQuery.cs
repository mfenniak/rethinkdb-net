using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class BetweenQuery<T> : ISequenceQuery<T>
    {
        private readonly ISequenceQuery<T> tableTerm;
        private readonly string leftKeyString;
        private readonly string rightKeyString;
        private readonly double? leftKeyNumber;
        private readonly double? rightKeyNumber;

        public BetweenQuery(ISequenceQuery<T> tableTerm, string leftKey, string rightKey)
        {
            this.tableTerm = tableTerm;
            this.leftKeyString = leftKey;
            this.rightKeyString = rightKey;
        }

        public BetweenQuery(ISequenceQuery<T> tableTerm, double? leftKey, double? rightKey)
        {
            this.tableTerm = tableTerm;
            this.leftKeyNumber = leftKey;
            this.rightKeyNumber = rightKey;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var betweenTerm = new Term()
            {
                type = Term.TermType.BETWEEN,
            };
            betweenTerm.args.Add(tableTerm.GenerateTerm(datumConverterFactory));

            if (leftKeyString != null)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "left_bound",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = leftKeyString,
                        }
                    }
                });
            }

            if (rightKeyString != null)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "right_bound",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = rightKeyString,
                        }
                    }
                });
            }

            if (leftKeyNumber.HasValue)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "left_bound",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = leftKeyNumber.Value,
                        }
                    }
                });
            }

            if (rightKeyNumber.HasValue)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "right_bound",
                    val = new Term() {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = rightKeyNumber.Value,
                        }
                    }
                });
            }

            return betweenTerm;
        }
    }
}