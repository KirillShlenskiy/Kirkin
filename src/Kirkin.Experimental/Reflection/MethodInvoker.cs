using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Provides functionality similar to <see cref="System.Reflection.MethodInfo"/> Invoke method.
    /// Leverages compiled expression trees to improve on reflection's performance while maintaining a similar API.
    /// </summary>
    public sealed class MethodInvoker
    {
        private Func<object, object[], object> CompiledDelegate;

        /// <summary>
        /// Method invoked by this instance.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Creates a new <see cref="MethodInvoker"/> instance.
        /// </summary>
        public MethodInvoker(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

            MethodInfo = methodInfo;
        }

        /// <summary>
        /// Invokes the target method on the given object instance (pass null if the method is static).
        /// </summary>
        public object Invoke(object instance, params object[] arguments)
        {
            if (CompiledDelegate == null) {
                CompileDelegate();
            }

            return CompiledDelegate(instance, arguments);
        }

        // Expensive, so not calling it in constructor.
        private void CompileDelegate()
        {
            ParameterExpression argumentsExpression = Expression.Parameter(typeof(object[]), "args");
            ParameterInfo[] parameters = MethodInfo.GetParameters();
            Expression[] argumentExpressions = new Expression[parameters.Length];

            for (int i = 0; i < parameters.Length; ++i)
            {
                argumentExpressions[i] = Expression.Convert(
                    Expression.ArrayIndex(argumentsExpression, Expression.Constant(i)),
                    parameters[i].ParameterType
                );
            }

            // Unfortunately needs to be allocated even for static method calls.
            ParameterExpression instanceExpression = Expression.Parameter(typeof(object), "obj");

            MethodCallExpression callExpression = MethodInfo.IsStatic
                ? Expression.Call(null, MethodInfo, argumentExpressions)
                : Expression.Call(Expression.Convert(instanceExpression, MethodInfo.ReflectedType), MethodInfo, argumentExpressions);

            if (MethodInfo.ReturnType == typeof(void))
            {
                Action<object, object[]> voidDelegate = Expression
                    .Lambda<Action<object, object[]>>(callExpression, instanceExpression, argumentsExpression)
                    .Compile();

                CompiledDelegate = (instance, arguments) =>
                {
                    voidDelegate(instance, arguments);

                    return null;
                };
            }
            else
            {
                CompiledDelegate = Expression
                    .Lambda<Func<object, object[], object>>(Expression.Convert(callExpression, typeof(object)), instanceExpression, argumentsExpression)
                    .Compile();
            }
        }
    }
}