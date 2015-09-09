using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RethinkDb.QueryTerm
{
    public class CompoundIndexCreateQuery<TTable> : IndexCreateQuery<TTable, object[]>
    {
        public CompoundIndexCreateQuery(ITableQuery<TTable> tableTerm, string indexName, Expression<Func<TTable, object[]>> indexExpression) 
            : base(tableTerm, indexName, indexExpression, false)
        {
        }
    }
}
