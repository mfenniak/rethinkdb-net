using System;

namespace RethinkDb
{
    public static class SynchronousApi
    {
        public static IConnection Get(this IConnectionFactory connectionFactory)
        {
            return TaskUtilities.ExecuteSynchronously(() => connectionFactory.GetAsync());
        }
    }
}

