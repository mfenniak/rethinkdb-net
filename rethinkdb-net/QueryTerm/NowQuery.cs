using RethinkDb.Spec;
using System;

namespace RethinkDb.QueryTerm
{
    public class NowQuery<TResult> : ISingleObjectQuery<TResult>
    {
        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.NOW,
            };
            return term;
        }
    }
}