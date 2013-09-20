using System;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface ISequenceQuery<T> : IQuery
    {
    }
}

