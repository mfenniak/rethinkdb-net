using RethinkDb.Spec;
using System;

namespace RethinkDb.QueryTerm
{
    public class NowQuery : ISingleObjectQuery<DateTimeOffset>
    {
        public NowQuery()
        {
        }

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