using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DeleteQueryBase<T>
    {
        private readonly IMutableSingleObjectQuery<T> getTerm;
        private readonly ISequenceQuery<T> tableTerm;

        public DeleteQueryBase(IMutableSingleObjectQuery<T> getTerm)
        {
            this.getTerm = getTerm;
        }

        public DeleteQueryBase(ISequenceQuery<T> tableTerm)
        {
            this.tableTerm = tableTerm;
        }

        public Term GenerateTerm(IDatumConverterFactory datumConverterFactory)
        {
            var deleteTerm = new Term()
            {
                type = Term.TermType.DELETE,
            };
            if (getTerm != null)
                deleteTerm.args.Add(getTerm.GenerateTerm(datumConverterFactory));
            else if (tableTerm != null)
                deleteTerm.args.Add(tableTerm.GenerateTerm(datumConverterFactory));
            AddOptionalArguments(deleteTerm);
            return deleteTerm;
        }

        protected virtual void AddOptionalArguments(Term updateTerm)
        {
        }
    }
}

