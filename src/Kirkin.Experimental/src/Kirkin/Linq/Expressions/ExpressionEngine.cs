using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Linq.Expressions
{
    internal static class ExpressionEngine
    {
        public static Expression<Func<TObject, TField>> FieldGetter<TObject, TField>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

            ParameterExpression param = Expression.Parameter(typeof(TObject), "o");

            // o => o.Field;
            return Expression.Lambda<Func<TObject, TField>>(
                Expression.Field(param, fieldInfo),
                param
            );
        }

        public static Expression<Action<TObject, TField>> FieldSetter<TObject, TField>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

            ParameterExpression param = Expression.Parameter(typeof(TObject), "o");
            ParameterExpression value = Expression.Parameter(typeof(TField), "value");

            // (o, value) => o.Field = value;
            return Expression.Lambda<Action<TObject, TField>>(
                Expression.Assign(Expression.Field(param, fieldInfo), value),
                param,
                value
            );
        }
    }
}