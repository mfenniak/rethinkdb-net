using System;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IWriteQuery<TResponseType> : IScalarQuery<TResponseType>
    {
    }
}

