using RethinkDb.Spec;
using System.Collections.Generic;
using System;

namespace RethinkDb.QueryTerm
{
    public class BetweenQuery<T> : ISequenceQuery<T>
    {
        private readonly TableQuery<T> tableTerm;
        private readonly string leftKeyString;
        private readonly string rightKeyString;
        private readonly double? leftKeyNumber;
        private readonly double? rightKeyNumber;

        public BetweenQuery(TableQuery<T> tableTerm, string leftKey, string rightKey)
        {
            this.tableTerm = tableTerm;
            this.leftKeyString = leftKey;
            this.rightKeyString = rightKey;
        }

        public BetweenQuery(TableQuery<T> tableTerm, double? leftKey, double? rightKey)
        {
            this.tableTerm = tableTerm;
            this.leftKeyNumber = leftKey;
            this.rightKeyNumber = rightKey;
        }

        public DeleteQuery<T> Delete()
        {
            return new DeleteQuery<T>(this);
        }

        Spec.Term ISequenceQuery<T>.GenerateTerm()
        {
            var betweenTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.BETWEEN,
            };
            betweenTerm.args.Add(((ISequenceQuery<T>)tableTerm).GenerateTerm());

            if (leftKeyString != null)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "left_bound",
                    val = new Spec.Term() {
                        type = Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_STR,
                            r_str = leftKeyString,
                        }
                    }
                });
            }

            if (rightKeyString != null)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "right_bound",
                    val = new Spec.Term() {
                        type = Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_STR,
                            r_str = rightKeyString,
                        }
                    }
                });
            }

            if (leftKeyNumber.HasValue)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "left_bound",
                    val = new Spec.Term() {
                        type = Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_NUM,
                            r_num = leftKeyNumber.Value,
                        }
                    }
                });
            }

            if (rightKeyNumber.HasValue)
            {
                betweenTerm.optargs.Add(new Term.AssocPair() {
                    key = "right_bound",
                    val = new Spec.Term() {
                        type = Term.TermType.DATUM,
                        datum = new Spec.Datum()
                        {
                            type = Spec.Datum.DatumType.R_NUM,
                            r_num = rightKeyNumber.Value,
                        }
                    }
                });
            }

            return betweenTerm;
        }
    }
}