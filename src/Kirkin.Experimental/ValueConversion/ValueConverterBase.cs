using System;
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

        private readonly ConcurrentDictionary<Type, Func<object, object>> ConvertDelegatesByReturnType
            = new ConcurrentDictionary<Type, Func<object, object>>();

        private Func<object, object> ResolveConvertDelegate(Type type)
        {
            Func<object, object> func;

            return ConvertDelegatesByReturnType.TryGetValue(type, out func)
                ? func
                : ResolveConvertDelegateSlow(type);
        }

        private Func<object, object> ResolveConvertDelegateSlow(Type type)
        {
            MethodInfo interpretMethod = InstanceMethod<ValueConverter>(vc => vc.Convert<int>(null));

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

        private static MethodInfo InstanceMethod<T>(Expression<Action<T>> methodCall)
        {
            MethodCallExpression call = methodCall.Body as MethodCallExpression;

            if (call == null) {
                throw new InvalidOperationException($"The given expression is not a {nameof(MethodCallExpression)}");
            }

            return call.Method;
        }

        #endregion
    }
}