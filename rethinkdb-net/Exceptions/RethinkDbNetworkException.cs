using System;
using System.Runtime.Serialization;

namespace RethinkDb
{
    /// <summary>
    /// Exception thrown when network connectivity errors occur.
    /// </summary>
    [Serializable]
    public class RethinkDbNetworkException : RethinkDbException
    {
        internal RethinkDbNetworkException(string message)
            : base(message)
        {
        }

        internal RethinkDbNetworkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RethinkDbNetworkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

