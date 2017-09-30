using System;
using System.Linq.Expressions;

#if !NET_40
using System.Runtime.CompilerServices;
#endif

namespace Kirkin.Utilities
{
    /// <summary>
    /// Object factory utilities.
    /// </summary>
    public static class ObjectFactory
    {
        /// <summary>
        /// Efficient substitute for "new T()" in constrained generic methods.
        /// </summary>
#if !NET_40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T CreateInstance<T>()
            where T : new()
        {
            return ObjectFactoryImpl<T>.Instance();
        }

        private static class ObjectFactoryImpl<T>
        {
            // Cached "return new T()" delegate.
            public static readonly Func<T> Instance = CreateFactory();

            private static Func<T> CreateFactory()
            {
                NewExpression newExpr = Expression.New(typeof(T));

                return Expression
                    .Lambda<Func<T>>(newExpr)
                    .Compile();
            }
        }
    }
}