using System;

namespace RethinkDb.Json
{
    public class EndArrayToken : IToken
    {
        public static readonly EndArrayToken Instance = new EndArrayToken();

        private EndArrayToken()
        {
        }

        public TokenType Type
        {
            get { return TokenType.EndArray; }
        }
    }
}
