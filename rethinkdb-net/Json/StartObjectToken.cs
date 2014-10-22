using System;

namespace RethinkDb.Json
{
    public class StartObjectToken : IToken
    {
        public static readonly StartObjectToken Instance = new StartObjectToken();

        private StartObjectToken()
        {
        }

        public TokenType Type
        {
            get { return TokenType.StartObject; }
        }
    }
}
