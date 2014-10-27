using System;
using System.Runtime.Serialization;

namespace RethinkDb
{
    [Serializable]
    public abstract class RethinkDbException : Exception
    {
        protected RethinkDbException(string message)
            : base(message)
        {
        }

        protected RethinkDbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RethinkDbException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

