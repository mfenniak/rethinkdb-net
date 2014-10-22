using System;

namespace RethinkDb.Json
{
    public class PropertyNameToken : IToken
    {
        public PropertyNameToken(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        public string PropertyName
        {
            get;
            private set;
        }

        public TokenType Type
        {
            get { return TokenType.PropertyName; }
        }
    }
}
