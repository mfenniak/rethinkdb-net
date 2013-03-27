using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DeleteQuery<T> : IDmlQuery
    {
        private readonly IMutableSingleObjectQuery<T> getTerm;
        private readonly ISequenceQuery<T> tableTerm;

        public DeleteQuery(IMutableSingleObjectQuery<T> getTerm)
        {
            this.getTerm = getTerm;
        }

        public DeleteQuery(ISequenceQuery<T> tableTerm)
        {
            this.tableTerm = tableTerm;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var replaceTerm = new Term()
            {
                type = Term.TermType.DELETE,
            };
            if (getTerm != null)
                replaceTerm.args.Add(getTerm.GenerateTerm(datumConverterFactory));
            else if (tableTerm != null)
                replaceTerm.args.Add(tableTerm.GenerateTerm(datumConverterFactory));
            return replaceTerm;
        }
    }
}

