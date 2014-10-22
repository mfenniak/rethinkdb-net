using System;

namespace RethinkDb.Json
{
    public class NumberToken : IToken
    {
        public NumberToken(string number)
        {
            this.Number = number;
        }

        public string Number
        {
            get;
            private set;
        }

        public TokenType Type
        {
            get { return TokenType.Number; }
        }
    }
}
