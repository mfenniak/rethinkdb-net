using System;

namespace RethinkDb.Json
{
    public class StringToken : IToken
    {
        public StringToken(string str)
        {
            this.String = str;
        }

        public string String
        {
            get;
            private set;
        }

        public TokenType Type
        {
            get { return TokenType.String; }
        }
    }
}
