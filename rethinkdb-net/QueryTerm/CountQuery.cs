using RethinkDb.Spec;
using System;

namespace RethinkDb.QueryTerm
{
    public class CountQuery<T> : ISingleObjectQuery<double>
    {
        private readonly ISequenceQuery<T> sequenceQuery;

        public CountQuery(ISequenceQuery<T> sequenceQuery)
        {
            this.sequenceQuery = sequenceQuery;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var countTerm = new Term()
            {
                type = Term.TermType.COUNT,
            };
            countTerm.args.Add(sequenceQuery.GenerateTerm(datumConverterFactory));
            return countTerm;
        }
    }
}