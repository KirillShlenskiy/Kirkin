using System;
using System.Linq.Expressions;

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
        public static T Create<T>()
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