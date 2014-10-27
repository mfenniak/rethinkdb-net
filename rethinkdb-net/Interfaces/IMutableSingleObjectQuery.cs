using System;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IMutableSingleObjectQuery<T> : ISingleObjectQuery<T>
    {
    }
}

