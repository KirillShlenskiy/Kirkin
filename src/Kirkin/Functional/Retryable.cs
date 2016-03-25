using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Functional
{
    /// <summary>
    /// Base class for types which create delegates with retry functionality baked in.
    /// </summary>
    public abstract class Retryable<TDelegate>
    {
        internal readonly TDelegate Delegate;

        internal Retryable(TDelegate del)
        {
            Delegate = del;
        }

        /// <summary>
        /// Returns a delegate which keeps retrying the execution
        /// allowing for the given number of exceptions of any type.
        /// </summary>
        public TDelegate OnException(int maxRetries)
        {
            return OnException<Exception>(maxRetries, CancellationToken.None);
        }

        /// <summary>
        /// Returns a delegate which keeps retrying the execution
        /// allowing for the given number of exceptions of any type
        /// and observing the given <see cref="CancellationToken" />.
        /// </summary>
        public TDelegate OnException(int maxRetries, CancellationToken ct)
        {
            return OnException<Exception>(maxRetries, ct);
        }

        /// <summary>
        /// Returns a delegate which keeps retrying the execution allowing
        /// for the given number of exceptions derived from the given type.
        /// </summary>
        public TDelegate OnException<TException>(int maxRetries)
            where TException : Exception
        {
            return OnException<TException>(maxRetries, CancellationToken.None);
        }

        /// <summary>
        /// Returns a delegate which keeps retrying the execution allowing
        /// for the given number of exceptions derived from the given type
        /// and observing the given <see cref="CancellationToken" />.
        /// </summary>
        public abstract TDelegate OnException<TException>(int maxRetries, CancellationToken ct)
            where TException : Exception;
    }

    /// <summary>
    /// <see cref="Retryable{TDelegate}" /> factory methods.
    /// </summary>
    public static class Retryable
    {
        /// <summary>
        /// Returns a retryable version of the given delegate.
        /// </summary>
        public static Retryable<Action> Retry(this Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            return new ActionRetryable(action);
        }

        /// <summary>
        /// Returns a retryable version of the given delegate.
        /// </summary>
        public static Retryable<Func<T>> Retry<T>(this Func<T> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            if (typeof(Task).IsAssignableFrom(typeof(T))) {
                throw new InvalidOperationException("Task-producing delegate detected. You are looking for the other overload.");
            }

            return new FuncRetryable<T>(func);
        }

#if !NET_40
        /// <summary>
        /// Returns a retryable version of the given async delegate.
        /// </summary>
        public static Retryable<Func<Task>> Retry(this Func<Task> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            return new TaskActionRetryable(action);
        }

        /// <summary>
        /// Returns a retryable version of the given async delegate.
        /// </summary>
        public static Retryable<Func<Task<T>>> Retry<T>(this Func<Task<T>> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            return new TaskFuncRetryable<T>(func);
        }
#endif
        sealed class ActionRetryable : Retryable<Action>
        {
            internal ActionRetryable(Action del)
                : base(del)
            {
            }

            public override Action OnException<TException>(int maxRetries, CancellationToken ct)
            {
                return () =>
                {
                    int currentTry = 0;

                    while (true)
                    {
                        try
                        {
                            ct.ThrowIfCancellationRequested();
                            Delegate();

                            return;
                        }
                        catch (TException)
                        {
                            if (currentTry++ == maxRetries) {
                                throw;
                            }
                        }
                    }

                    throw new InvalidOperationException();
                };
            }
        }

        sealed class FuncRetryable<T> : Retryable<Func<T>>
        {
            internal FuncRetryable(Func<T> del)
                : base(del)
            {
            }

            public override Func<T> OnException<TException>(int maxRetries, CancellationToken ct)
            {
                return () =>
                {
                    int currentTry = 0;

                    while (true)
                    {
                        try
                        {
                            ct.ThrowIfCancellationRequested();

                            return Delegate();
                        }
                        catch (TException)
                        {
                            if (currentTry++ == maxRetries) {
                                throw;
                            }
                        }
                    }

                    throw new InvalidOperationException();
                };
            }
        }

#if !NET_40
        sealed class TaskActionRetryable : Retryable<Func<Task>>
        {
            internal TaskActionRetryable(Func<Task> del)
                : base(del)
            {
            }

            public override Func<Task> OnException<TException>(int maxRetries, CancellationToken ct)
            {
                return async () =>
                {
                    int currentTry = 0;

                    while (true)
                    {
                        try
                        {
                            ct.ThrowIfCancellationRequested();

                            await Delegate().ConfigureAwait(false);

                            return;
                        }
                        catch (TException)
                        {
                            if (currentTry++ == maxRetries) {
                                throw;
                            }
                        }
                    }

                    throw new InvalidOperationException();
                };
            }
        }

        sealed class TaskFuncRetryable<T> : Retryable<Func<Task<T>>>
        {
            internal TaskFuncRetryable(Func<Task<T>> del)
                : base(del)
            {
            }

            public override Func<Task<T>> OnException<TException>(int maxRetries, CancellationToken ct)
            {
                return async () =>
                {
                    int currentTry = 0;

                    while (true)
                    {
                        try
                        {
                            ct.ThrowIfCancellationRequested();

                            return await Delegate().ConfigureAwait(false);
                        }
                        catch (TException)
                        {
                            if (currentTry++ == maxRetries) {
                                throw;
                            }
                        }
                    }

                    throw new InvalidOperationException();
                };
            }
        }
#endif
    }
}