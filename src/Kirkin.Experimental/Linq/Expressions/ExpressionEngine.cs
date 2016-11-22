//#define CACHING

using System;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Collections.Generic;

#if CACHING
using System.Collections.Concurrent;
using Kirkin.Reflection;
#endif

namespace Kirkin.Linq.Expressions
{
    internal static class ExpressionEngine
    {
        static class Cache<T>
        {
            internal static readonly ParameterExpression Param = Expression.Parameter(typeof(T), "o");
#if CACHING
            internal static readonly ConcurrentDictionary<MemberInfo, Expression> Getters
                = new ConcurrentDictionary<MemberInfo, Expression>(MemberInfoEqualityComparer.Instance);

            internal static readonly ConcurrentDictionary<MemberInfo, Expression> Setters
                = new ConcurrentDictionary<MemberInfo, Expression>(MemberInfoEqualityComparer.Instance);
#endif
        }

        /// <summary>
        /// Creates an expression which represents reading a field or getting the value of a property.
        /// </summary>
        public static Expression<Func<TObject, TMember>> Getter<TObject, TMember>(MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            Expression expression;
#if CACHING
            if (!Cache<TObject>.Getters.TryGetValue(memberInfo, out expression))
            {
#endif
                ParameterExpression param = Cache<TObject>.Param;

                // o => o.Field;
                expression = Expression.Lambda<Func<TObject, TMember>>(
                    Expression.MakeMemberAccess(param, member),
                    param
                );
#if CACHING
                Cache<TObject>.Getters.TryAdd(memberInfo, expression);
            }
#endif
            return (Expression<Func<TObject, TMember>>)expression;
        }

        /// <summary>
        /// Creates an expression which represents writing a field or setting the value of a property.
        /// </summary>
        public static Expression<Action<TObject, TMember>> Setter<TObject, TMember>(MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            Expression expression;
#if CACHING
            if (!Cache<TObject>.Setters.TryGetValue(memberInfo, out expression))
            {
#endif
                ParameterExpression param = Cache<TObject>.Param;
                ParameterExpression value = Expression.Parameter(typeof(TMember), "value");

                // (o, value) => o.Field = value;
                expression = Expression.Lambda<Action<TObject, TMember>>(
                    Expression.Assign(Expression.MakeMemberAccess(param, member), value),
                    param,
                    value
                );
#if CACHING
                Cache<TObject>.Setters.TryAdd(memberInfo, expression);
            }
#endif
            return (Expression<Action<TObject, TMember>>)expression;
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the given constructor.
        /// </summary>
        /// <typeparam name="TDelegate">
        /// Type of delegate described by the resultant expression. Must take in the
        /// list of parameters matching the given constructor, and return an object of
        /// type which defines the given constructor.
        /// </typeparam>
        public static Expression<TDelegate> Constructor<TDelegate>(ConstructorInfo constructor)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));

            ParameterInfo[] parameters = constructor.GetParameters();

            if (parameters.Length != 0)
            {
                ParameterExpression[] parameterExpressions = new ParameterExpression[parameters.Length];

                for (int i = 0; i < parameters.Length; i++) {
                    parameterExpressions[i] = Expression.Parameter(parameters[i].ParameterType);
                }

                return Expression.Lambda<TDelegate>(Expression.New(constructor, parameterExpressions), parameterExpressions);
            }

            return Expression.Lambda<TDelegate>(Expression.New(constructor));
        }

        /// <summary>
        /// Returns the instance constructor resolution helper for type <typeparamref name="T"/>.
        /// </summary>
        public static ConstructorResolutionHelper<T> Constructor<T>()
        {
            return ConstructorResolutionHelper<T>.Instance;
        }

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

                return Constructor<TDelegate>(constructor);
            }
        }

        /// <summary>
        /// Creates an expression which represents the invocation of the given instance method.
        /// </summary>
        /// <typeparam name="TDelegate">
        /// Type of delegate described by the resultant expression. The delegate's
        /// list of parameters and return type must match the given instance method.
        /// </typeparam>
        public static Expression<TDelegate> Method<TDelegate>(MethodInfo method)
        {
            ParameterExpression instance = Expression.Parameter(method.DeclaringType, "o");
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != 0)
            {
                ParameterExpression[] parameterExpressions = new ParameterExpression[parameters.Length];

                for (int i = 0; i < parameters.Length; i++) {
                    parameterExpressions[i] = Expression.Parameter(parameters[i].ParameterType);
                }

                ParameterExpression[] parameterExpressionsPrefixedByInstance = new ParameterExpression[parameterExpressions.Length + 1];

                parameterExpressionsPrefixedByInstance[0] = instance;

                for (int i = 0; i < parameterExpressions.Length; i++) {
                    parameterExpressionsPrefixedByInstance[i + 1] = parameterExpressions[i];
                }

                return Expression.Lambda<TDelegate>(Expression.Call(instance, method, parameterExpressions), parameterExpressionsPrefixedByInstance);
            }

            return Expression.Lambda<TDelegate>(Expression.Call(instance, method), instance);
        }

        /// <summary>
        /// Returns the instance method resolution helper for type <typeparamref name="T"/>.
        /// </summary>
        public static MethodResolutionHelper<T> Method<T>()
        {
            return MethodResolutionHelper<T>.Instance;
        }

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

                return Method<TDelegate>(method);
            }
        }
    }
}