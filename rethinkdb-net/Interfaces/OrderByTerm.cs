using System;
using System.Linq.Expressions;

namespace RethinkDb
{
    public class OrderByTerm<T>
    {
        public Expression<Func<T, object>> Expression;
        public OrderByDirection Direction;
        public string IndexName;
    }
}
