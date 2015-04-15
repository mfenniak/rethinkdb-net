using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class ChangesQuery<TRecord> : IStreamingSequenceQuery<DmlResponseChange<TRecord>>
    {
        private readonly IChangefeedCompatibleQuery<TRecord> changefeedCompatibleQuery;

        public ChangesQuery(IChangefeedCompatibleQuery<TRecord> changefeedCompatibleQuery)
        {
            this.changefeedCompatibleQuery = changefeedCompatibleQuery;
        }

        public Term GenerateTerm(IQueryConverter queryConverter)
        {
            var term = new Term()
            {
                type = Term.TermType.CHANGES,
            };
            term.args.Add(changefeedCompatibleQuery.GenerateTerm(queryConverter));
            return term;
        }
    }
}
