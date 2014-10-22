using System;

namespace RethinkDb.Json
{
    public class TrueToken : IToken
    {
        public static readonly TrueToken Instance = new TrueToken();

        private TrueToken()
        {
        }

        public TokenType Type
        {
            get { return TokenType.True; }
        }
    }
}
