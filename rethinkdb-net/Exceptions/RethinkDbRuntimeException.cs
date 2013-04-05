using System;
using System.Runtime.Serialization;

namespace RethinkDb
{
    /// <summary>
    /// Exception thrown when runtime query failure occurs.  For example, if a query attempts to add two fields
    /// together, but at runtime they are types that don't support that operation (like booleans).
    /// </summary>
    [Serializable]
    public class RethinkDbRuntimeException : RethinkDbException
    {
        internal RethinkDbRuntimeException(string message)
            : base(message)
        {
        }

        internal RethinkDbRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RethinkDbRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

