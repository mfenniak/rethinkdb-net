using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RethinkDb
{
    public class MultiIndex<TRecord, TIndex> : IMultiIndex<TRecord, TIndex>
    {
        private readonly ITableQuery<TRecord> table;
        private readonly string name;
        private readonly Expression<Func<TRecord, IEnumerable<TIndex>>> indexAccessor;

        public MultiIndex(ITableQuery<TRecord> table, string name, Expression<Func<TRecord, IEnumerable<TIndex>>> indexAccessor)
        {
            this.table = table;
            this.name = name;
            this.indexAccessor = indexAccessor;
        }

        public ITableQuery<TRecord> Table
        {
            get { return table; }
        }

        public string Name
        {
            get { return name; }
        }

        public Expression<Func<TRecord, IEnumerable<TIndex>>> IndexAccessor
        {
            get { return indexAccessor; }
        }
    }
}
