using RethinkDb.Spec;
using System;

namespace RethinkDb.QueryTerm
{
    public class SampleQuery<T> : ISequenceQuery<T>
    {
        private readonly ISequenceQuery<T> sequenceQuery;
        private readonly int number;

        public SampleQuery(ISequenceQuery<T> sequenceQuery, int number)
        {
            this.sequenceQuery = sequenceQuery;
            this.number = number;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory, IExpressionConverterFactory expressionConverterFactory)
        {
            var sampleTerm = new Term()
            {
                type = Term.TermType.SAMPLE,
            };
            sampleTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory, expressionConverterFactory));
            sampleTerm.args.Add(
                new Term()
                {
                    type = Term.TermType.DATUM,
                    datum = new Datum()
                    {
                        type = Datum.DatumType.R_NUM,
                        r_num = number,
                    }
                }
            );
            return sampleTerm;
        }
    }
}