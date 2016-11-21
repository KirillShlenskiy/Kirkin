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
        /// Returns the constructor resolution helper for type <typeparamref name="T"/>.
        /// </summary>
        public static ConstructorResolution<T> Constructor<T>()
        {
            return ConstructorResolution<T>.Instance;
        }

        /// <summary>
        /// Constructor expression resolution helper type.
        /// </summary>
        public sealed class ConstructorResolution<T>
        {
            internal static readonly ConstructorResolution<T> Instance = new ConstructorResolution<T>();

            private ConstructorResolution()
            {
            }

            /// <summary>
            /// Creates an expression which represents the invocation of the
            /// parameterless constructor of type <typeparamref name="T"/>.
            /// </summary>
            /// <param name="nonPublic">True if non-public constructors can be matched.</param>
            public Expression<Func<T>> Parameterless(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(Array<Type>.Empty, nonPublic);

                return Constructor<Func<T>>(constructor);
            }

            /// <summary>
            /// Creates an expression which represents the invocation of the
            /// constructor of type <typeparamref name="T"/> with the given parameter.
            /// </summary>
            /// <param name="nonPublic">True if non-public constructors can be matched.</param>
            public Expression<Func<TParam, T>> WithParameters<TParam>(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(new[] { typeof(TParam) }, nonPublic);

                return Constructor<Func<TParam, T>>(constructor);
            }

            /// <summary>
            /// Creates an expression which represents the invocation of the
            /// constructor of type <typeparamref name="T"/> with the given parameters.
            /// </summary>
            /// <param name="nonPublic">True if non-public constructors can be matched.</param>
            public Expression<Func<TParam1, TParam2, T>> WithParameters<TParam1, TParam2>(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(new[] { typeof(TParam1), typeof(TParam2) }, nonPublic);

                return Constructor<Func<TParam1, TParam2, T>>(constructor);
            }

            /// <summary>
            /// Creates an expression which represents the invocation of the
            /// constructor of type <typeparamref name="T"/> with the given parameters.
            /// </summary>
            /// <param name="nonPublic">True if non-public constructors can be matched.</param>
            public Expression<Func<TParam1, TParam2, TParam3, T>> WithParameters<TParam1, TParam2, TParam3>(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3) }, nonPublic);

                return Constructor<Func<TParam1, TParam2, TParam3, T>>(constructor);
            }

            /// <summary>
            /// Creates an expression which represents the invocation of the
            /// constructor of type <typeparamref name="T"/> with the given parameters.
            /// </summary>
            /// <param name="nonPublic">True if non-public constructors can be matched.</param>
            public Expression<Func<TParam1, TParam2, TParam3, TParam4, T>> WithParameters<TParam1, TParam2, TParam3, TParam4>(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(new[] { typeof(TParam1), typeof(TParam2), typeof(TParam3), typeof(TParam4) }, nonPublic);

                return Constructor<Func<TParam1, TParam2, TParam3, TParam4, T>>(constructor);
            }

            private static ConstructorInfo ConstructorWithGivenParameters(Type[] parameterTypes, bool nonPublic)
            {
                BindingFlags bindingFlags = nonPublic
                    ? BindingFlags.Instance | BindingFlags.NonPublic
                    : BindingFlags.Instance;

                ConstructorInfo constructor = typeof(T).GetConstructor(bindingFlags, null, parameterTypes, null);

                if (constructor == null) {
                    throw new InvalidOperationException("Unable to resolve the constructor with matching parameters.");
                }

                return constructor;
            }
        }
    }
}