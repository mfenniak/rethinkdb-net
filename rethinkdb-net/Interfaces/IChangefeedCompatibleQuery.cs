using System;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IChangefeedCompatibleQuery<T> : IQuery
    {
    }
}
