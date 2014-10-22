using System;

namespace RethinkDb.Json
{
    public class NullToken : IToken
    {
        public static readonly NullToken Instance = new NullToken();

        private NullToken()
        {
        }

        public TokenType Type
        {
            get { return TokenType.Null; }
        }
    }
}
