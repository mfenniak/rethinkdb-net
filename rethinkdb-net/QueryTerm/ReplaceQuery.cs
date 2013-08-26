using RethinkDb.Spec;

namespace RethinkDb.QueryTerm
{
    public class ReplaceQuery<T> : ReplaceQueryBase<T>, IWriteQuery<DmlResponse>
    {
        public ReplaceQuery(IMutableSingleObjectQuery<T> getTerm, T newObject, bool nonAtomic)
            : base(getTerm, newObject, nonAtomic)
        {
        }
    }
}

