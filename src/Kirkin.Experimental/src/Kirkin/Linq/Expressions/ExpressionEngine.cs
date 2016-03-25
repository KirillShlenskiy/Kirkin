using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Linq.Expressions
{
    internal static class ExpressionEngine
    {
        static class Cache<T>
        {
            internal static readonly ParameterExpression Param = Expression.Parameter(typeof(T), "o");
        }

        /// <summary>
        /// Creates an expression which represents reading from the given field.
        /// </summary>
        public static Expression<Func<TObject, TField>> FieldGetter<TObject, TField>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

            ParameterExpression param = Cache<TObject>.Param;

            // o => o.Field;
            return Expression.Lambda<Func<TObject, TField>>(
                Expression.Field(param, fieldInfo),
                param
            );
        }

        /// <summary>
        /// Creates an expression which represents writing to the given field.
        /// </summary>
        public static Expression<Action<TObject, TField>> FieldSetter<TObject, TField>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

            ParameterExpression param = Cache<TObject>.Param;
            ParameterExpression value = Expression.Parameter(typeof(TField), "value");

            // (o, value) => o.Field = value;
            return Expression.Lambda<Action<TObject, TField>>(
                Expression.Assign(Expression.Field(param, fieldInfo), value),
                param,
                value
            );
        }

        /// <summary>
        /// Creates an expression which represents getting the value of the given property.
        /// </summary>
        public static Expression<Func<TObject, TProperty>> PropertyGetter<TObject, TProperty>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            ParameterExpression param = Cache<TObject>.Param;

            // o => o.Property;
            return Expression.Lambda<Func<TObject, TProperty>>(
                Expression.Property(param, propertyInfo),
                param
            );
        }

        /// <summary>
        /// Creates an expression which represents setting the value of the given property.
        /// </summary>
        public static Expression<Action<TObject, TProperty>> PropertySetter<TObject, TProperty>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            ParameterExpression param = Cache<TObject>.Param;
            ParameterExpression value = Expression.Parameter(typeof(TProperty), "value");

            // (o, value) => o.Property = value;
            return Expression.Lambda<Action<TObject, TProperty>>(
                Expression.Assign(Expression.Property(param, propertyInfo), value),
                param,
                value
            );
        }
    }
}