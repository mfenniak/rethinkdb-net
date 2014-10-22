using System;

namespace RethinkDb.Json
{
    public class StartArrayToken : IToken
    {
        public static readonly StartArrayToken Instance = new StartArrayToken();

        private StartArrayToken()
        {
        }

        public TokenType Type
        {
            get { return TokenType.StartArray; }
        }
    }
}
