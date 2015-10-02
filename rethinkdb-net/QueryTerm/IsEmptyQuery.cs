using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class IsEmptyQuery<T> : IScalarQuery<bool>
    {
        private readonly ISequenceQuery<T> sequenceQuery;

        public IsEmptyQuery(ISequenceQuery<T> sequenceQuery)
        {
            this.sequenceQuery = sequenceQuery;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term
            {
                type = Term.TermType.IS_EMPTY
            };
            
            term.args.Add(sequenceQuery.GenerateTerm(queryConverter));

            return term;
        }
    }
}
