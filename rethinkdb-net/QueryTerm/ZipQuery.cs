using RethinkDb.Spec;
using System;
using System.Linq.Expressions;

namespace RethinkDb.QueryTerm
{
    public class ZipQuery<TJoinedType, TTarget> : ISequenceQuery<TTarget>
    {
        private ISequenceQuery<TJoinedType> sequenceQuery;

        public ZipQuery(ISequenceQuery<TJoinedType> sequenceQuery)
        {
            this.sequenceQuery = sequenceQuery;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var zipTerm = new Term()
            {
                type = Term.TermType.ZIP,
            };
            zipTerm.args.Add(sequenceQuery.GenerateTerm(queryConverter));
            return zipTerm;
        }
    }
}
