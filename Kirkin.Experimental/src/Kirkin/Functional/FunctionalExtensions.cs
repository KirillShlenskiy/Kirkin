using System;

namespace Kirkin.Functional
{
    /// <summary>
    /// Allows Kirill to stay productive
    /// and have fun while programming
    /// in a slightly more functional way.
    /// </summary>
    public static class FunctionalExtensions
    {
        /// <summary>
        /// Enables simple inline operations on objects of any type.
        /// </summary>
        public static T Apply<T>(this T input, Action<T> action)
        {
            action(input);

            return input;
        }

        /// <summary>
        /// Enables simple inline functional transformations on objects of any type.
        /// </summary>
        public static TResult Apply<TInput, TResult>(this TInput input, Func<TInput, TResult> transformation)
        {
            return transformation(input);
        }
    }
}