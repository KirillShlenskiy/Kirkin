﻿//#define CACHING

using System;
using System.Collections.Generic;
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

        internal static ConstructorResolution<T> Constructor<T>()
        {
            return ConstructorResolution<T>.Instance;
        }

        internal sealed class ConstructorResolution<T>
        {
            internal static readonly ConstructorResolution<T> Instance = new ConstructorResolution<T>();

            private ConstructorResolution()
            {
            }

            public Expression<Func<T>> Parameterless(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(Array<Type>.Empty, nonPublic);

                return Constructor<Func<T>>(constructor);
            }

            public Expression<Func<TArg, T>> WithArguments<TArg>(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(new[] { typeof(TArg) }, nonPublic);

                return Constructor<Func<TArg, T>>(constructor);
            }

            public Expression<Func<TArg1, TArg2, T>> WithArguments<TArg1, TArg2>(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(new[] { typeof(TArg1), typeof(TArg2) }, nonPublic);

                return Constructor<Func<TArg1, TArg2, T>>(constructor);
            }

            public Expression<Func<TArg1, TArg2, TArg3, T>> WithArguments<TArg1, TArg2, TArg3>(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, nonPublic);

                return Constructor<Func<TArg1, TArg2, TArg3, T>>(constructor);
            }

            public Expression<Func<TArg1, TArg2, TArg3, TArg4, T>> WithArguments<TArg1, TArg2, TArg3, TArg4>(bool nonPublic = false)
            {
                ConstructorInfo constructor = ConstructorWithGivenParameters(new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, nonPublic);

                return Constructor<Func<TArg1, TArg2, TArg3, TArg4, T>>(constructor);
            }

            private static ConstructorInfo ConstructorWithGivenParameters(Type[] parameterTypes, bool nonPublic)
            {
                BindingFlags bindingFlags = nonPublic
                    ? BindingFlags.Instance | BindingFlags.NonPublic
                    : BindingFlags.Instance;

                return typeof(T).GetConstructor(bindingFlags, null, parameterTypes, null);
            }
        }
    }
}