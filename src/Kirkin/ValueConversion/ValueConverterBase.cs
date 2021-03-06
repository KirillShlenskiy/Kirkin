﻿using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.ValueConversion
{
    /// <summary>
    /// Base class for types which perform value conversions from one type to another.
    /// </summary>
    public abstract class ValueConverterBase
    {
        #region Public methods

        /// <summary>
        /// Converts the value to the given type.
        /// </summary>
        public abstract T Convert<T>(object value);

        /// <summary>
        /// Converts the value to the given type.
        /// </summary>
        public object Convert(object value, Type type)
        {
            return ResolveConvertDelegate(type)(value);
        }

        #endregion

        #region Conversion delegate resolution

        private static readonly MethodInfo OpenGenericConvertMethod
            = typeof(ValueConverter).GetMethod(nameof(Convert), new[] { typeof(object) });

        private readonly ConcurrentDictionary<Type, Func<object, object>> ConvertDelegatesByReturnType
            = new ConcurrentDictionary<Type, Func<object, object>>();

        private Func<object, object> ResolveConvertDelegate(Type type)
        {
            return ConvertDelegatesByReturnType.TryGetValue(type, out Func<object, object> func)
                ? func
                : ResolveConvertDelegateSlow(type);
        }

        private Func<object, object> ResolveConvertDelegateSlow(Type type)
        {
            MethodInfo interpretMethod = OpenGenericConvertMethod.MakeGenericMethod(type);

            // Lambda expression:
            // (object value) => (object)this.Convert<T>(value);
            ParameterExpression valueExpr = Expression.Parameter(typeof(object), "value");

            Expression body = Expression.Convert(
                Expression.Call(Expression.Constant(this, GetType()), interpretMethod, valueExpr),
                typeof(object)
            );

            Func<object, object> func = Expression
                .Lambda<Func<object, object>>(body, valueExpr)
                .Compile();

            return ConvertDelegatesByReturnType.GetOrAdd(type, func);
        }

        #endregion
    }
}