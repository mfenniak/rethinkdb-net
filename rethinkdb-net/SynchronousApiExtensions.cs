using System;
using System.Collections.Generic;

namespace RethinkDb
{
    public static class SynchronousApiExtensions
    {
        #region IConnectionFactory

        public static IConnection Get(this IConnectionFactory connectionFactory)
        {
            return TaskUtilities.ExecuteSynchronously(() => connectionFactory.GetAsync());
        }

        #endregion
        #region IConnectableConnection

        public static void Connect(this IConnectableConnection connection)
        {
            TaskUtilities.ExecuteSynchronously(() => connection.ConnectAsync());
        }

        #endregion
        #region IConnection

        public static T Run<T>(this IConnection connection, IDatumConverterFactory datumConverterFactory, IScalarQuery<T> queryObject)
        {
            return TaskUtilities.ExecuteSynchronously(() => connection.RunAsync<T>(datumConverterFactory, queryObject));
        }

        public static T Run<T>(this IConnection connection, IScalarQuery<T> queryObject)
        {
            return TaskUtilities.ExecuteSynchronously(() => connection.RunAsync<T>(queryObject));
        }

        public static IEnumerable<T> Run<T>(this IConnection connection, IDatumConverterFactory datumConverterFactory, ISequenceQuery<T> queryObject)
        {
            return new AsyncEnumerableSynchronizer<T>(() => connection.RunAsync<T>(datumConverterFactory, queryObject));
        }

        public static IEnumerable<T> Run<T>(this IConnection connection, ISequenceQuery<T> queryObject)
        {
            return new AsyncEnumerableSynchronizer<T>(() => connection.RunAsync<T>(queryObject));
        }

        #endregion
        #region AsyncEnumerable synchronous wrappers

        private class AsyncEnumerableSynchronizer<T> : IEnumerable<T>
        {
            private readonly Func<IAsyncEnumerator<T>> asyncEnumeratorFactory;

            public AsyncEnumerableSynchronizer(Func<IAsyncEnumerator<T>> asyncEnumeratorFactory)
            {
                this.asyncEnumeratorFactory = asyncEnumeratorFactory;
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new AsyncEnumeratorSynchronizer<T>(asyncEnumeratorFactory());
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new AsyncEnumeratorSynchronizer<T>(asyncEnumeratorFactory());
            }
        }

        private sealed class AsyncEnumeratorSynchronizer<T> : IEnumerator<T>
        {
            private IAsyncEnumerator<T> asyncEnumerator;

            public AsyncEnumeratorSynchronizer(IAsyncEnumerator<T> asyncEnumerator)
            {
                this.asyncEnumerator = asyncEnumerator;
            }

            #region IDisposable implementation

            public void Dispose()
            {
                if (this.asyncEnumerator != null)
                {
                    TaskUtilities.ExecuteSynchronously(() => this.asyncEnumerator.Dispose());
                    this.asyncEnumerator = null;
                }
            }

            #endregion
            #region IEnumerator implementation

            public bool MoveNext()
            {
                if (asyncEnumerator == null)
                    throw new ObjectDisposedException(GetType().FullName);
                return TaskUtilities.ExecuteSynchronously(() => asyncEnumerator.MoveNext());
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public object Current
            {
                get
                {
                    if (asyncEnumerator == null)
                        throw new ObjectDisposedException(GetType().FullName);
                    return asyncEnumerator.Current;
                }
            }

            #endregion
            #region IEnumerator implementation

            T IEnumerator<T>.Current
            {
                get
                {
                    if (asyncEnumerator == null)
                        throw new ObjectDisposedException(GetType().FullName);
                    return asyncEnumerator.Current;
                }
            }

            #endregion
        }

        #endregion
    }
}

