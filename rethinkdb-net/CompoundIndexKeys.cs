using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RethinkDb
{
    public class CompoundIndexKey
    {
        protected CompoundIndexKey(params object[] keyValues)
        {
            this.KeyValues = keyValues;
        }

        public object[] KeyValues
        {
            get;
            set;
        }
    }

    public class CompoundIndexKey<TKey1, TKey2> : CompoundIndexKey
    {
        public CompoundIndexKey(TKey1 key1, TKey2 key2)
            : base(key1, key2)
        {
        }
    }

    public class CompoundIndexKey<TKey1, TKey2, TKey3> : CompoundIndexKey
    {
        public CompoundIndexKey(TKey1 key1, TKey2 key2, TKey3 key3)
            : base(key1, key2, key3)
        {
        }
    }

    public class CompoundIndexKey<TKey1, TKey2, TKey3, TKey4> : CompoundIndexKey
    {
        public CompoundIndexKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
            : base(key1, key2, key3, key4)
        {
        }
    }

    public class CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5> : CompoundIndexKey
    {
        public CompoundIndexKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
            : base(key1, key2, key3, key4, key5)
        {
        }
    }

    public class CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> : CompoundIndexKey
    {
        public CompoundIndexKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6)
            : base(key1, key2, key3, key4, key5, key6)
        {
        }
    }

    public class CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> : CompoundIndexKey
    {
        public CompoundIndexKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7)
            : base(key1, key2, key3, key4, key5, key6, key7)
        {
        }
    }

    public class CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8> : CompoundIndexKey
    {
        public CompoundIndexKey(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, TKey8 key8)
            : base(key1, key2, key3, key4, key5, key6, key7, key8)
        {
        }
    }
}
