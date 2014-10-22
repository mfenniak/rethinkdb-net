using System;

namespace RethinkDb.Json
{
    public interface IToken
    {
        TokenType Type { get; }
    }
}
