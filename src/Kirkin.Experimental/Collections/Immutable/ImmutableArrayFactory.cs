using System;
using System.Collections.Immutable;

using Kirkin.Linq.Expressions;

namespace Kirkin.Collections.Immutable
{
    /// <summary>
    /// Factory type which facilitates the creation of an
    /// <see cref="ImmutableArray{T}"/> facade wrapping a mutable array.
    /// </summary>
    public static class ImmutableArrayFactory
    {
        /// <summary>
        /// Creates an <see cref="ImmutableArray{T}"/> wrapper around the given array.
        /// The resulting array won't be truly immutable, but the consumers will
        /// think it is and the performance will be better than copy can provide.
        /// </summary>
        public static ImmutableArray<T> WrapArray<T>(T[] array)
        {
            return ImmutableArrayConstructor<T>.Value(array);
        }

        static class ImmutableArrayConstructor<T>
        {
            private static Func<T[], ImmutableArray<T>> _value;

            internal static Func<T[], ImmutableArray<T>> Value
            {
                get
                {
                    if (_value == null)
                    {
                        // It's a safe bet to assume that ImmutableArray<T> will always
                        // provide a non-public constructor wrapping a mutable array.
                        _value = MemberExpressions
                            .Constructor<ImmutableArray<T>>()
                            .WithParameters<T[]>(nonPublic: true)
                            .Compile();
                    }

                    return _value;
                }
            }
        }
    }
}