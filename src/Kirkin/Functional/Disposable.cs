using System;
using System.Collections.Generic;
using System.Threading;

namespace Kirkin.Functional
{
    /// <summary>
    /// Commonly used IDisposable factory methods.
    /// </summary>
    public static class Disposable
    {
        /// <summary>
        /// Returns a disposable which does nothing when disposed.
        /// </summary>
        public static IDisposable Empty
        {
            get { return EmptyDisposable.Instance; }
        }

        /// <summary>
        /// Wraps multiple disposable resources
        /// into a single IDisposable instance.
        /// </summary>
        public static IDisposable Combine<TDisposable>(IEnumerable<TDisposable> disposables)
            where TDisposable : IDisposable
        {
            if (disposables == null) throw new ArgumentNullException("disposables");

            List<TDisposable> list = new List<TDisposable>();

            try
            {
                foreach (TDisposable disposable in disposables)
                {
                    list.Add(disposable);
                }
            }
            catch
            {
                // Dispose items already in the list.
                foreach (TDisposable disposable in list)
                {
                    disposable.Dispose();
                }

                throw;
            }

            return Create(list, l =>
            {
                foreach (TDisposable disposable in l)
                {
                    disposable.Dispose();
                }
            });
        }

        /// <summary>
        /// Returns a thread-safe disposable which
        /// guarantees that the Dispose action is
        /// executed at most once.
        /// </summary>
        public static IDisposable Create(Action disposeAction)
        {
            if (disposeAction == null) throw new ArgumentNullException("disposeAction");

            return new DisposableActor(disposeAction);
        }

        /// <summary>
        /// Returns a thread-safe disposable which
        /// guarantees that the Dispose action is
        /// executed at most once.
        /// </summary>
        public static IDisposable Create<T>(T arg, Action<T> disposeAction)
        {
            // It is legal for arg to be any value including null.
            if (disposeAction == null) throw new ArgumentNullException("disposeAction");

            return new ParametrizedDisposableActor<T>(arg, disposeAction);
        }

        /// <summary>
        /// Returns a simple disposable object
        /// which invokes the given action every
        /// time a call to Dispose is made.
        /// </summary>
        public static IDisposable Multi(Action disposeAction)
        {
            if (disposeAction == null) throw new ArgumentException("disposeAction");

            return new SimpleDisposable(disposeAction);
        }

        /// <summary>
        /// Simple disposable which invokes the given
        /// action every time Dispose is called.
        /// </summary>
        private sealed class SimpleDisposable : IDisposable
        {
            private readonly Action DisposeAction;

            public SimpleDisposable(Action disposeAction)
            {
                DisposeAction = disposeAction;
            }

            public void Dispose()
            {
                DisposeAction();
            }
        }

        /// <summary>
        /// Performs the specified operation when Dispose is called.
        /// </summary>
        private sealed class DisposableActor : IDisposable
        {
            private Action Action;

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            public DisposableActor(Action action)
            {
                Action = action;
            }

            /// <summary>
            /// Performs the action specified when this instance was created
            /// provided that Dispose has not already been called.
            /// </summary>
            public void Dispose()
            {
                Action action = Interlocked.Exchange(ref Action, null);

                if (action != null)
                {
                    action();
                }
            }
        }

        /// <summary>
        /// Performs the specified operation when Dispose is called.
        /// </summary>
        private sealed class ParametrizedDisposableActor<T> : IDisposable
        {
            private readonly T Arg;
            private Action<T> Action;

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            public ParametrizedDisposableActor(T arg, Action<T> action)
            {
                Arg = arg;
                Action = action;
            }

            /// <summary>
            /// Performs the action specified when this instance was created
            /// provided that Dispose has not already been called.
            /// </summary>
            public void Dispose()
            {
                Action<T> action = Interlocked.Exchange(ref Action, null);

                if (action != null)
                {
                    action(Arg);
                }
            }
        }

        /// <summary>
        /// Disposable which does nothing.
        /// </summary>
        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new EmptyDisposable();

            private EmptyDisposable()
            {
            }

            public void Dispose()
            {
                // Do nothing.
            }
        }
    }
}