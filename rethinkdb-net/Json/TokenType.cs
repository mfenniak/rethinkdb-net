using System;

namespace RethinkDb
{
    public enum TokenType
    {
        StartObject,
        PropertyName,
        EndObject,

        StartArray,
        EndArray,

        String,
        Number,
        True,
        False,
        Null,
    }
}
