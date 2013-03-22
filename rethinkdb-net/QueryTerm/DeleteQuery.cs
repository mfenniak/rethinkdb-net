using System;

namespace RethinkDb.QueryTerm
{
    public class DeleteQuery<T> : IDmlQuery
    {
        private readonly GetQuery<T> getTerm;
        private readonly TableQuery<T> tableTerm;

        public DeleteQuery(TableQuery<T> tableTerm)
        {
            this.tableTerm = tableTerm;
        }

        public DeleteQuery(GetQuery<T> getTerm)
        {
            this.getTerm = getTerm;
        }

        Spec.Term ISingleObjectQuery<DmlResponse>.GenerateTerm()
        {
            var replaceTerm = new Spec.Term()
            {
                type = Spec.Term.TermType.DELETE,
            };
            if (getTerm != null)
                replaceTerm.args.Add(((ISingleObjectQuery<T>)getTerm).GenerateTerm());
            else if (tableTerm != null)
                replaceTerm.args.Add(((ISequenceQuery<T>)tableTerm).GenerateTerm());
            return replaceTerm;
        }
    }
}

