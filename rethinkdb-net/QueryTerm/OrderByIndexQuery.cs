using System;

namespace RethinkDb.QueryTerm
{
    public class OrderByIndexQuery<T> : OrderByQuery<T>, IOrderByIndexQuery<T>
    {
        public OrderByIndexQuery(ISequenceQuery<T> sequenceQuery, params OrderByTerm<T>[] orderByMembers)
            : base(sequenceQuery, orderByMembers)
        {
        }
    }
}
