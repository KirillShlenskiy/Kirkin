using System;

namespace Kirkin.Functional
{
    /// <summary>
    /// Functional decorators.
    /// </summary>
    /// <example>
    /// Basic usage:
    /// <code>
    /// Func&lt;int, int&gt; funcPlusOne = Decorate.Func&lt;int, int&gt;(
    ///     (x, y) => x + y,
    ///     f => (x, y) => f(x, y) + 1
    /// );
    /// </code>
    /// </example>
    public static class Decorate
    {
        /// <summary>
        /// Decorates the given action.
        /// </summary>
        public static Action Action(Action action, Func<Action, Action> decorator)
        {
            return decorator(action);
        }

        /// <summary>
        /// Decorates the given action.
        /// </summary>
        public static Action<T> Action<T>(Action<T> action, Func<Action<T>, Action<T>> decorator)
        {
            return decorator(action);
        }

        /// <summary>
        /// Decorates the given action.
        /// </summary>
        public static Action<T1, T2> Action<T1, T2>(Action<T1, T2> action, Func<Action<T1, T2>, Action<T1, T2>> decorator)
        {
            return decorator(action);
        }

        /// <summary>
        /// Decorates the given action.
        /// </summary>
        public static Action<T1, T2, T3> Action<T1, T2, T3>(Action<T1, T2, T3> action, Func<Action<T1, T2, T3>, Action<T1, T2, T3>> decorator)
        {
            return decorator(action);
        }

        /// <summary>
        /// Decorates the given action.
        /// </summary>
        public static Action<T1, T2, T3, T4> Action<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, Func<Action<T1, T2, T3, T4>, Action<T1, T2, T3, T4>> decorator)
        {
            return decorator(action);
        }

        /// <summary>
        /// Decorates the given function.
        /// </summary>
        public static Func<T> Func<T>(Func<T> func, Func<Func<T>, Func<T>> decorator)
        {
            return decorator(func);
        }

        /// <summary>
        /// Decorates the given function.
        /// </summary>
        public static Func<T, TResult> Func<T, TResult>(Func<T, TResult> func, Func<Func<T, TResult>, Func<T, TResult>> decorator)
        {
            return decorator(func);
        }

        /// <summary>
        /// Decorates the given function.
        /// </summary>
        public static Func<T1, T2, TResult> Func<T1, T2, TResult>(Func<T1, T2, TResult> func, Func<Func<T1, T2, TResult>, Func<T1, T2, TResult>> decorator)
        {
            return decorator(func);
        }

        /// <summary>
        /// Decorates the given function.
        /// </summary>
        public static Func<T1, T2, T3, TResult> Func<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, Func<Func<T1, T2, T3, TResult>, Func<T1, T2, T3, TResult>> decorator)
        {
            return decorator(func);
        }

        /// <summary>
        /// Decorates the given function.
        /// </summary>
        public static Func<T1, T2, T3, T4, TResult> Func<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, Func<Func<T1, T2, T3, T4, TResult>, Func<T1, T2, T3, T4, TResult>> decorator)
        {
            return decorator(func);
        }
    }
}