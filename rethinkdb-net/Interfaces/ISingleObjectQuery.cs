using System;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface ISingleObjectQuery<T> : IScalarQuery<T>
    {
    }
}

