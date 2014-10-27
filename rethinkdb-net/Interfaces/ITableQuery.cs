using System;
using System.ComponentModel;
using RethinkDb.Spec;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface ITableQuery<T> : ISequenceQuery<T>
    {
    }
}

