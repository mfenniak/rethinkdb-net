using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class ReplaceQuery<T> : IWriteQuery<T>
    {
        private readonly IMutableSingleObjectQuery<T> getTerm;
        private readonly T newObject;

        public ReplaceQuery(IMutableSingleObjectQuery<T> getTerm, T newObject)
        {
            this.getTerm = getTerm;
            this.newObject = newObject;
        }

        public Term GenerateTerm(IDatumConverter<T> converter)
        {
            var replaceTerm = new Term()
            {
                type = Term.TermType.REPLACE,
            };
            replaceTerm.args.Add(getTerm.GenerateTerm());
            replaceTerm.args.Add(new Term() {
                type = Term.TermType.DATUM,
                datum = converter.ConvertObject(newObject)
            });
            return replaceTerm;
        }
    }
}

