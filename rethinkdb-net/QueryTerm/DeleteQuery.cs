using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DeleteQuery<T> : IDmlQuery
    {
        private readonly GetQuery<T> getTerm;
        private readonly ISequenceQuery<T> tableTerm;

        public DeleteQuery(GetQuery<T> getTerm)
        {
            this.getTerm = getTerm;
        }

        public DeleteQuery(ISequenceQuery<T> tableTerm)
        {
            this.tableTerm = tableTerm;
        }

        public Term GenerateTerm()
        {
            var replaceTerm = new Term()
            {
                type = Term.TermType.DELETE,
            };
            if (getTerm != null)
                replaceTerm.args.Add(getTerm.GenerateTerm());
            else if (tableTerm != null)
                replaceTerm.args.Add(tableTerm.GenerateTerm());
            return replaceTerm;
        }
    }
}

