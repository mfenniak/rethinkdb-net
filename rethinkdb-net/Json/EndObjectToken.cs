using System;

namespace RethinkDb.Json
{
    public class EndObjectToken : IToken
    {
        public static readonly EndObjectToken Instance = new EndObjectToken();

        private EndObjectToken()
        {
        }

        public TokenType Type
        {
            get { return TokenType.EndObject; }
        }
    }
}
