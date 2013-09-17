using System;
using System.Threading;
using System.Threading.Tasks;

namespace RethinkDb
{
    static class TaskUtilities
    {
        /// <summary>
        /// Execute a void Task synchronously; this method is passed a delegate to create the task.
        /// </summary>
        /// <remarks>
        /// Unlike calling the task's Wait() method, this method will discard the current synchronization context
        /// before creating the task.  This will prevent any internal "await"'s from attempting to use the same
        /// synchronization context, which can cause deadlocks (issue #130) depending upon the synchronization context
        /// implementation.
        /// </remarks>
        public static void ExecuteSynchronously(Func<Task> taskDelegate)
        {
            var synchronizationContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                var task = taskDelegate();
                task.Wait();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }
        }

        /// <summary>
        /// Execute a void Task synchronously; this method is passed a delegate to create the task.
        /// </summary>
        /// <remarks>
        /// Unlike calling the task's Wait()/Result, this method will discard the current synchronization context
        /// before creating the task.  This will prevent any internal "await"'s from attempting to use the same
        /// synchronization context, which can cause deadlocks (issue #130) depending upon the synchronization context
        /// implementation.
        /// </remarks>
        public static T ExecuteSynchronously<T>(Func<Task<T>> taskDelegate)
        {
            var synchronizationContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                var task = taskDelegate();
                return task.Result;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }
        }
    }
}
