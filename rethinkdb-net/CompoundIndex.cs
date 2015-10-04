using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

namespace RethinkDb
{
    public class CompoundIndexBase<TRecord>
    {
        private readonly ITableQuery<TRecord> table;
        private readonly string name;
        private readonly Expression[] indexExpressions;

        protected CompoundIndexBase(ITableQuery<TRecord> table, string name, Expression[] indexExpressions)
        {
            this.table = table;
            this.name = name;
            this.indexExpressions = indexExpressions;
        }

        public Expression[] IndexAccessor
        {
            get { return indexExpressions; }
        }

        public ITableQuery<TRecord> Table
        {
            get { return table; }
        }

        public string Name
        {
            get { return name; }
        }
    }

    public class CompoundIndex<TRecord, TKey1, TKey2> : CompoundIndexBase<TRecord>, ICompoundIndex<TRecord, TKey1, TKey2>
    {
        public CompoundIndex(ITableQuery<TRecord> table, string name, Expression<Func<TRecord, TKey1>> indexExpression1, Expression<Func<TRecord, TKey2>> indexExpression2)
            : base(table, name, new Expression[] { indexExpression1, indexExpression2 })
        {
        }

        public CompoundIndexKey<TKey1, TKey2> Key(TKey1 key1, TKey2 key2)
        {
            return new CompoundIndexKey<TKey1, TKey2>(key1, key2);
        }
    }

    public class CompoundIndex<TRecord, TKey1, TKey2, TKey3> : CompoundIndexBase<TRecord>, ICompoundIndex<TRecord, TKey1, TKey2, TKey3>
    {
        public CompoundIndex(ITableQuery<TRecord> table, string name, Expression<Func<TRecord, TKey1>> indexExpression1, Expression<Func<TRecord, TKey2>> indexExpression2, Expression<Func<TRecord, TKey3>> indexExpression3)
            : base(table, name, new Expression[] { indexExpression1, indexExpression2, indexExpression3 })
        {
        }

        public CompoundIndexKey<TKey1, TKey2, TKey3> Key(TKey1 key1, TKey2 key2, TKey3 key3)
        {
            return new CompoundIndexKey<TKey1, TKey2, TKey3>(key1, key2, key3);
        }
    }

    public class CompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4> : CompoundIndexBase<TRecord>, ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4>
    {
        public CompoundIndex(ITableQuery<TRecord> table, string name, Expression<Func<TRecord, TKey1>> indexExpression1, Expression<Func<TRecord, TKey2>> indexExpression2, Expression<Func<TRecord, TKey3>> indexExpression3, Expression<Func<TRecord, TKey4>> indexExpression4)
            : base(table, name, new Expression[] { indexExpression1, indexExpression2, indexExpression3, indexExpression4 })
        {
        }

        public CompoundIndexKey<TKey1, TKey2, TKey3, TKey4> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
        {
            return new CompoundIndexKey<TKey1, TKey2, TKey3, TKey4>(key1, key2, key3, key4);
        }
    }

    public class CompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5> : CompoundIndexBase<TRecord>, ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5>
    {
        public CompoundIndex(ITableQuery<TRecord> table, string name, Expression<Func<TRecord, TKey1>> indexExpression1, Expression<Func<TRecord, TKey2>> indexExpression2, Expression<Func<TRecord, TKey3>> indexExpression3, Expression<Func<TRecord, TKey4>> indexExpression4, Expression<Func<TRecord, TKey5>> indexExpression5)
            : base(table, name, new Expression[] { indexExpression1, indexExpression2, indexExpression3, indexExpression4, indexExpression5 })
        {
        }

        public CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
        {
            return new CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5>(key1, key2, key3, key4, key5);
        }
    }

    public class CompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> : CompoundIndexBase<TRecord>, ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>
    {
        public CompoundIndex(ITableQuery<TRecord> table, string name, Expression<Func<TRecord, TKey1>> indexExpression1, Expression<Func<TRecord, TKey2>> indexExpression2, Expression<Func<TRecord, TKey3>> indexExpression3, Expression<Func<TRecord, TKey4>> indexExpression4, Expression<Func<TRecord, TKey5>> indexExpression5, Expression<Func<TRecord, TKey6>> indexExpression6)
            : base(table, name, new Expression[] { indexExpression1, indexExpression2, indexExpression3, indexExpression4, indexExpression5, indexExpression6 })
        {
        }

        public CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6)
        {
            return new CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(key1, key2, key3, key4, key5, key6);
        }
    }

    public class CompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> : CompoundIndexBase<TRecord>, ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>
    {
        public CompoundIndex(ITableQuery<TRecord> table, string name, Expression<Func<TRecord, TKey1>> indexExpression1, Expression<Func<TRecord, TKey2>> indexExpression2, Expression<Func<TRecord, TKey3>> indexExpression3, Expression<Func<TRecord, TKey4>> indexExpression4, Expression<Func<TRecord, TKey5>> indexExpression5, Expression<Func<TRecord, TKey6>> indexExpression6, Expression<Func<TRecord, TKey7>> indexExpression7)
            : base(table, name, new Expression[] { indexExpression1, indexExpression2, indexExpression3, indexExpression4, indexExpression5, indexExpression6, indexExpression7 })
        {
        }

        public CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7)
        {
            return new CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>(key1, key2, key3, key4, key5, key6, key7);
        }
    }

    public class CompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8> : CompoundIndexBase<TRecord>, ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8>
    {
        public CompoundIndex(ITableQuery<TRecord> table, string name, Expression<Func<TRecord, TKey1>> indexExpression1, Expression<Func<TRecord, TKey2>> indexExpression2, Expression<Func<TRecord, TKey3>> indexExpression3, Expression<Func<TRecord, TKey4>> indexExpression4, Expression<Func<TRecord, TKey5>> indexExpression5, Expression<Func<TRecord, TKey6>> indexExpression6, Expression<Func<TRecord, TKey7>> indexExpression7, Expression<Func<TRecord, TKey8>> indexExpression8)
            : base(table, name, new Expression[] { indexExpression1, indexExpression2, indexExpression3, indexExpression4, indexExpression5, indexExpression6, indexExpression7, indexExpression8 })
        {
        }

        public CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, TKey8 key8)
        {
            return new CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8>(key1, key2, key3, key4, key5, key6, key7, key8);
        }
    }
}
