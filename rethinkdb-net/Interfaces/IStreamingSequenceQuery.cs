using System;
using System.ComponentModel;

namespace RethinkDb
{
    [ImmutableObject(true)]
    public interface IStreamingSequenceQuery<T> : ISequenceQuery<T>
    {
    }
}
