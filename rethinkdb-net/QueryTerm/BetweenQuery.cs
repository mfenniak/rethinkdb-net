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
                betweenTerm.args.Add(new Term() 
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = leftKeyString,
                        }
                    }
                );
            }

            if (leftKeyNumber.HasValue)
            {
                betweenTerm.args.Add(new Term() 
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = leftKeyNumber.Value,
                        }
                    }
                );
            }

            if (leftKeyString == null && !leftKeyNumber.HasValue)
            {
                betweenTerm.args.Add(GenerateNullDatum());
            }

            if (rightKeyString != null)
            {
                betweenTerm.args.Add(new Term() 
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_STR,
                            r_str = rightKeyString,
                        }
                    }
                );
            }

            if (rightKeyNumber.HasValue)
            {
                betweenTerm.args.Add(new Term() 
                    {
                        type = Term.TermType.DATUM,
                        datum = new Datum()
                        {
                            type = Datum.DatumType.R_NUM,
                            r_num = rightKeyNumber.Value,
                        }
                    }
                );
            }

            if (rightKeyString == null && !rightKeyNumber.HasValue)
            {
                betweenTerm.args.Add(GenerateNullDatum());
            }

            return betweenTerm;
        }

        private static Term GenerateNullDatum()
        {
            return new Term() 
            {
                type = Term.TermType.DATUM,
                datum = new Datum() 
                {
                    type = Datum.DatumType.R_NULL
                }
            };
        }
    }
}