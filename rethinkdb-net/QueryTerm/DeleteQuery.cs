using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class DeleteQuery<T> : DeleteQueryBase<T>, ISingleObjectQuery<DmlResponse>
    {
        public DeleteQuery(IMutableSingleObjectQuery<T> getTerm)
            : base(getTerm)
        {
        }

        public DeleteQuery(ISequenceQuery<T> tableTerm)
            : base(tableTerm)
        {
        }
    }
}

