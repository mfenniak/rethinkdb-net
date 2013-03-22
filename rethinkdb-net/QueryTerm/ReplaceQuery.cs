using System;

namespace RethinkDb.QueryTerm
{
    public class ReplaceQuery<T> : IWriteQuery<T>
    {
        private readonly GetQuery<T> getTerm;
        private readonly T newObject;

        public ReplaceQuery(GetQuery<T> getTerm, T newObject)
        {
            this.getTerm = getTerm;
            this.newObject = newObject;
        }

        Spec.Term IWriteQuery<T>.GenerateTerm(IDatumConverter<T> converter)
        {
            var replaceTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.REPLACE,
            };
            replaceTerm.args.Add(((ISingleObjectQuery<T>)getTerm).GenerateTerm());
            replaceTerm.args.Add(new Spec.Term() {
                type = RethinkDb.Spec.Term.TermType.DATUM,
                datum = converter.ConvertObject(newObject)
            });
            return replaceTerm;
        }
    }
}

