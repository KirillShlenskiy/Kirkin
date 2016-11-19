//#define CACHING

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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

        internal static Expression<TDelegate> Constructor<TDelegate>(ConstructorInfo constructor)
        {
            Type delegateType = typeof(TDelegate);

            if (!delegateType.IsGenericType) throw new ArgumentException("Delegate type must be generic.");

            Type[] genericArgs = delegateType.GetGenericArguments();

            if (genericArgs.Length > 1)
            {
                List<ParameterExpression> parameters = new List<ParameterExpression>(genericArgs.Length - 1);

                for (int i = 0; i < genericArgs.Length - 1; i++) {
                    parameters.Add(Expression.Parameter(genericArgs[i]));
                }

                return Expression.Lambda<TDelegate>(Expression.New(constructor), parameters);
            }

            return Expression.Lambda<TDelegate>(Expression.New(constructor));
        }
    }
}