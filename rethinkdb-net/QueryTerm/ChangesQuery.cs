using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class ChangesQuery<TRecord> : IStreamingSequenceQuery<DmlResponseChange<TRecord>>
    {
        private readonly ISequenceQuery<TRecord> sequenceQuery;

        public ChangesQuery(ISequenceQuery<TRecord> sequenceQuery)
        {
            this.sequenceQuery = sequenceQuery;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.CHANGES,
            };
            term.args.Add(sequenceQuery.GenerateTerm(queryConverter));
            return term;
        }
    }
}
