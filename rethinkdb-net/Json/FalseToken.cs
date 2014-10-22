using System;

namespace RethinkDb.Json
{
    public class FalseToken : IToken
    {
        public static readonly FalseToken Instance = new FalseToken();

        private FalseToken()
        {
        }

        public TokenType Type
        {
            get { return TokenType.False; }
        }
    }
}
