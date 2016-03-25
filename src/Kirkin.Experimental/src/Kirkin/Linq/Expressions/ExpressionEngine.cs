//#define CACHING

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Reflection;

namespace Kirkin.Linq.Expressions
{
    internal static class ExpressionEngine
    {
        static class Cache<T>
        {
            internal static readonly ParameterExpression Param = Expression.Parameter(typeof(T), "o");

            internal static readonly ConcurrentDictionary<MemberInfo, Expression> Getters
                = new ConcurrentDictionary<MemberInfo, Expression>(MemberInfoEqualityComparer.Instance);

            internal static readonly ConcurrentDictionary<MemberInfo, Expression> Setters
                = new ConcurrentDictionary<MemberInfo, Expression>(MemberInfoEqualityComparer.Instance);
        }

        /// <summary>
        /// Creates an expression which represents reading from the given field.
        /// </summary>
        public static Expression<Func<TObject, TField>> FieldGetter<TObject, TField>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

            Expression expression;
#if CACHING
            if (!Cache<TObject>.Getters.TryGetValue(fieldInfo, out expression))
            {
#endif
                ParameterExpression param = Cache<TObject>.Param;

                // o => o.Field;
                expression = Expression.Lambda<Func<TObject, TField>>(
                    Expression.Field(param, fieldInfo),
                    param
                );
#if CACHING
                Cache<TObject>.Getters.TryAdd(fieldInfo, expression);
            }
#endif
            return (Expression<Func<TObject, TField>>)expression;
        }

        /// <summary>
        /// Creates an expression which represents writing to the given field.
        /// </summary>
        public static Expression<Action<TObject, TField>> FieldSetter<TObject, TField>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

            Expression expression;
#if CACHING
            if (!Cache<TObject>.Setters.TryGetValue(fieldInfo, out expression))
            {
#endif
                ParameterExpression param = Cache<TObject>.Param;
                ParameterExpression value = Expression.Parameter(typeof(TField), "value");

                // (o, value) => o.Field = value;
                expression = Expression.Lambda<Action<TObject, TField>>(
                    Expression.Assign(Expression.Field(param, fieldInfo), value),
                    param,
                    value
                );
#if CACHING
                Cache<TObject>.Setters.TryAdd(fieldInfo, expression);
            }
#endif
            return (Expression<Action<TObject, TField>>)expression;
        }

        /// <summary>
        /// Creates an expression which represents getting the value of the given property.
        /// </summary>
        public static Expression<Func<TObject, TProperty>> PropertyGetter<TObject, TProperty>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            Expression expression;
#if CACHING
            if (!Cache<TObject>.Getters.TryGetValue(propertyInfo, out expression))
            {
#endif
                ParameterExpression param = Cache<TObject>.Param;

                // o => o.Property;
                expression = Expression.Lambda<Func<TObject, TProperty>>(
                    Expression.Property(param, propertyInfo),
                    param
                );
#if CACHING
                Cache<TObject>.Getters.TryAdd(propertyInfo, expression);
            }
#endif
            return (Expression<Func<TObject, TProperty>>)expression;
        }

        /// <summary>
        /// Creates an expression which represents setting the value of the given property.
        /// </summary>
        public static Expression<Action<TObject, TProperty>> PropertySetter<TObject, TProperty>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            Expression expression;
#if CACHING
            if (!Cache<TObject>.Setters.TryGetValue(propertyInfo, out expression))
            {
#endif
                ParameterExpression param = Cache<TObject>.Param;
                ParameterExpression value = Expression.Parameter(typeof(TProperty), "value");

                // (o, value) => o.Property = value;
                expression = Expression.Lambda<Action<TObject, TProperty>>(
                    Expression.Assign(Expression.Property(param, propertyInfo), value),
                    param,
                    value
                );
#if CACHING
                Cache<TObject>.Setters.TryAdd(propertyInfo, expression);
            }
#endif
            return (Expression<Action<TObject, TProperty>>)expression;
        }
    }
}