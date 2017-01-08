using System;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Collections.Generic;

namespace Kirkin.Linq.Expressions.Fluent
{
    /// <summary>
    /// Instance constructor expression resolution helper type.
    /// </summary>
    public sealed class ConstructorResolutionHelper<T>
    {
        internal static readonly ConstructorResolutionHelper<T> Instance = new ConstructorResolutionHelper<T>();

        private ConstructorResolutionHelper()
        {
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the
        /// parameterless instance constructor of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="nonPublic">True if non-public constructors can be matched.</param>
        public Expression<Func<T>> Parameterless(bool nonPublic = false)
        {
            return ConstructorWithGivenParameters<Func<T>>(Array<Type>.Empty, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// constructor of type <typeparamref name="T"/> with the given parameter.
        /// </summary>
        /// <param name="nonPublic">True if non-public constructors can be matched.</param>
        public Expression<Func<TParam, T>> WithParameters<TParam>(bool nonPublic = false)
        {
            return ConstructorWithGivenParameters<Func<TParam, T>>(new[] { typeof(TParam) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// constructor of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public constructors can be matched.</param>
        public Expression<Func<TParam1, TParam2, T>> WithParameters<TParam1, TParam2>(bool nonPublic = false)
        {
            return ConstructorWithGivenParameters<Func<TParam1, TParam2, T>>(new[] { typeof(TParam1), typeof(TParam2) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// constructor of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public constructors can be matched.</param>
        public Expression<Func<TParam1, TParam2, TParam3, T>> WithParameters<TParam1, TParam2, TParam3>(bool nonPublic = false)
        {
            return ConstructorWithGivenParameters<Func<TParam1, TParam2, TParam3, T>>(
                new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// constructor of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public constructors can be matched.</param>
        public Expression<Func<TParam1, TParam2, TParam3, TParam4, T>> WithParameters<TParam1, TParam2, TParam3, TParam4>(bool nonPublic = false)
        {
            return ConstructorWithGivenParameters<Func<TParam1, TParam2, TParam3, TParam4, T>>(
                new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }, nonPublic);
        }

        private static Expression<TDelegate> ConstructorWithGivenParameters<TDelegate>(Type[] parameterTypes, bool nonPublic)
        {
            BindingFlags bindingFlags = nonPublic
                ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                : BindingFlags.Instance | BindingFlags.Public;

            ConstructorInfo constructor = typeof(T).GetConstructor(bindingFlags, null, parameterTypes, null);

            if (constructor == null) {
                throw new InvalidOperationException("Unable to resolve the constructor with matching parameters.");
            }

            return MemberExpressions.Constructor<TDelegate>(constructor);
        }
    }
}