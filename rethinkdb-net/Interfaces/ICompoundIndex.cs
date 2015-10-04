using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface ICompoundIndex<TRecord>
    {
        ITableQuery<TRecord> Table { get; }
        string Name { get; }
        Expression[] IndexAccessor { get; }
    }

    [ImmutableObject(true)]
    public interface ICompoundIndex<TRecord, TKey1, TKey2> : IBaseIndex<TRecord, CompoundIndexKey<TKey1, TKey2>>, ICompoundIndex<TRecord>
    {
        CompoundIndexKey<TKey1, TKey2> Key(TKey1 key1, TKey2 key2);
    }

    [ImmutableObject(true)]
    public interface ICompoundIndex<TRecord, TKey1, TKey2, TKey3> : IBaseIndex<TRecord, CompoundIndexKey<TKey1, TKey2, TKey3>>, ICompoundIndex<TRecord>
    {
        CompoundIndexKey<TKey1, TKey2, TKey3> Key(TKey1 key1, TKey2 key2, TKey3 key3);
    }

    [ImmutableObject(true)]
    public interface ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4> : IBaseIndex<TRecord, CompoundIndexKey<TKey1, TKey2, TKey3, TKey4>>, ICompoundIndex<TRecord>
    {
        CompoundIndexKey<TKey1, TKey2, TKey3, TKey4> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4);
    }

    [ImmutableObject(true)]
    public interface ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5> : IBaseIndex<TRecord, CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5>>, ICompoundIndex<TRecord>
    {
        CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5);
    }

    [ImmutableObject(true)]
    public interface ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> : IBaseIndex<TRecord, CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>>, ICompoundIndex<TRecord>
    {
        CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6);
    }

    [ImmutableObject(true)]
    public interface ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> : IBaseIndex<TRecord, CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>>, ICompoundIndex<TRecord>
    {
        CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7);
    }

    [ImmutableObject(true)]
    public interface ICompoundIndex<TRecord, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8> : IBaseIndex<TRecord, CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8>>, ICompoundIndex<TRecord>
    {
        CompoundIndexKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TKey8> Key(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7, TKey8 key8);
    }
}
