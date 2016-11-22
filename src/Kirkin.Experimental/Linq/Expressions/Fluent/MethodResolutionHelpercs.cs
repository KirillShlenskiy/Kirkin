using System;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Collections.Generic;

namespace Kirkin.Linq.Expressions.Fluent
{
    /// <summary>
    /// Instance method expression resolution helper type.
    /// </summary>
    public sealed class MethodResolutionHelper<T>
    {
        internal static readonly MethodResolutionHelper<T> Instance = new MethodResolutionHelper<T>();

        private MethodResolutionHelper()
        {
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with no parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Action<T>> Void(string name, bool nonPublic = false)
        {
            return InstanceMethod<Action<T>>(name, Array<Type>.Empty, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Action<T, TParam>> Void<TParam>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Action<T, TParam>>(name, new[] { typeof(TParam) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Action<T, TParam1, TParam2>> Void<TParam1, TParam2>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Action<T, TParam1, TParam2>>(name, new[] { typeof(TParam1), typeof(TParam2) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Action<T, TParam1, TParam2, TParam3>> Void<TParam1, TParam2, TParam3>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Action<T, TParam1, TParam2, TParam3>>(
                name, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Action<T, TParam1, TParam2, TParam3, TParam4>> Void<TParam1, TParam2, TParam3, TParam4>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Action<T, TParam1, TParam2, TParam3, TParam4>>(
                name, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with no parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Func<T, TReturn>> Func<TReturn>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Func<T, TReturn>>(name, Array<Type>.Empty, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Func<T, TParam, TReturn>> Func<TParam, TReturn>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Func<T, TParam, TReturn>>(name, new[] { typeof(TParam) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Func<T, TParam1, TParam2, TReturn>> Func<TParam1, TParam2, TReturn>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Func<T, TParam1, TParam2, TReturn>>(name, new[] { typeof(TParam1), typeof(TParam2) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Func<T, TParam1, TParam2, TParam3, TReturn>> Func<TParam1, TParam2, TParam3, TReturn>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Func<T, TParam1, TParam2, TParam3, TReturn>>(
                name, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, nonPublic);
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the instance
        /// method of type <typeparamref name="T"/> with the given parameters.
        /// </summary>
        /// <param name="nonPublic">True if non-public methods can be matched.</param>
        public Expression<Func<T, TParam1, TParam2, TParam3, TParam4, TReturn>> Func<TParam1, TParam2, TParam3, TParam4, TReturn>(string name, bool nonPublic = false)
        {
            return InstanceMethod<Func<T, TParam1, TParam2, TParam3, TParam4, TReturn>>(
                name, new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }, nonPublic);
        }

        private static Expression<TDelegate> InstanceMethod<TDelegate>(string name, Type[] parameterTypes, bool nonPublic)
        {
            BindingFlags bindingFlags = nonPublic
                ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                : BindingFlags.Instance | BindingFlags.Public;

            MethodInfo method = typeof(T).GetMethod(name, bindingFlags, null, parameterTypes, null);

            if (method == null) {
                throw new InvalidOperationException("Unable to resolve instance method with matching parameters.");
            }

            return ExpressionEngine.Method<TDelegate>(method);
        }
    }
}