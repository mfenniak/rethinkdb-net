using System;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IScalarQuery<T> : IQuery
    {
    }
}

