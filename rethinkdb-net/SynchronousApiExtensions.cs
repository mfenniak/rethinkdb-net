using System;
using System.Collections.Generic;
using System.Threading;

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

        public static void Connect(this IConnectableConnection connection, CancellationToken? cancellationToken = null)
        {
            TaskUtilities.ExecuteSynchronously(() => connection.ConnectAsync(cancellationToken));
        }

        #endregion
        #region IConnection

        public static T Run<T>(this IConnection connection, IScalarQuery<T> queryObject, IDatumConverterFactory datumConverterFactory = null, IExpressionConverterFactory expressionConverterFactory = null, CancellationToken? cancellationToken = null)
        {
            return TaskUtilities.ExecuteSynchronously(() => connection.RunAsync<T>(queryObject, datumConverterFactory, expressionConverterFactory, cancellationToken));
        }

        public static IEnumerable<T> Run<T>(this IConnection connection, ISequenceQuery<T> queryObject, IDatumConverterFactory datumConverterFactory = null, IExpressionConverterFactory expressionConverterFactory = null, CancellationToken? cancellationToken = null)
        {
            return new AsyncEnumerableSynchronizer<T>(() => connection.RunAsync<T>(queryObject, datumConverterFactory, expressionConverterFactory), cancellationToken);
        }

        #endregion
        #region AsyncEnumerable synchronous wrappers

        private class AsyncEnumerableSynchronizer<T> : IEnumerable<T>
        {
            private readonly Func<IAsyncEnumerator<T>> asyncEnumeratorFactory;
            private readonly CancellationToken? cancellationToken;

            public AsyncEnumerableSynchronizer(Func<IAsyncEnumerator<T>> asyncEnumeratorFactory, CancellationToken? cancellationToken)
            {
                this.asyncEnumeratorFactory = asyncEnumeratorFactory;
                this.cancellationToken = cancellationToken;
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return new AsyncEnumeratorSynchronizer<T>(asyncEnumeratorFactory(), cancellationToken);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new AsyncEnumeratorSynchronizer<T>(asyncEnumeratorFactory(), cancellationToken);
            }
        }

        private sealed class AsyncEnumeratorSynchronizer<T> : IEnumerator<T>
        {
            private IAsyncEnumerator<T> asyncEnumerator;
            private readonly CancellationToken? cancellationToken;

            public AsyncEnumeratorSynchronizer(IAsyncEnumerator<T> asyncEnumerator, CancellationToken? cancellationToken)
            {
                this.asyncEnumerator = asyncEnumerator;
                this.cancellationToken = cancellationToken;
            }

            #region IDisposable implementation

            public void Dispose()
            {
                if (this.asyncEnumerator != null)
                {
                    TaskUtilities.ExecuteSynchronously(() => this.asyncEnumerator.Dispose(cancellationToken));
                    this.asyncEnumerator = null;
                }
            }

            #endregion
            #region IEnumerator implementation

            public bool MoveNext()
            {
                if (asyncEnumerator == null)
                    throw new ObjectDisposedException(GetType().FullName);
                return TaskUtilities.ExecuteSynchronously(() => asyncEnumerator.MoveNext(cancellationToken));
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

