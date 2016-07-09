using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Reflection
{
    public sealed class Method
    {
        public static Method InstanceMethod<T>(Expression<Action<T>> methodCall)
        {
            MethodCallExpression call = methodCall.Body as MethodCallExpression;

            if (call == null) {
                throw new InvalidOperationException($"The given expression is not a {nameof(MethodCallExpression)}");
            }

            return new Method(methodCall, call);
        }

        public static Method StaticMethod(Expression<Action> methodCall)
        {
            MethodCallExpression call = methodCall.Body as MethodCallExpression;

            if (call == null) {
                throw new InvalidOperationException($"The given expression is not a {nameof(MethodCallExpression)}");
            }

            return new Method(methodCall, call);
        }

        private readonly LambdaExpression Lambda;
        private readonly MethodCallExpression MethodCall;

        private Method(LambdaExpression lambda, MethodCallExpression methodCall)
        {
            Lambda = lambda;
            MethodCall = methodCall;
        }

        public MethodCallExpression Expression
        {
            get
            {
                return MethodCall;
            }
        }

        public MethodInfo MethodInfo
        {
            get
            {
                return MethodCall.Method;
            }
        }

        public TDelegate CreateDelegate<TDelegate>()
        {
            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), MethodInfo);
        }

        public TDelegate CreateDelegate<TDelegate>(object instance)
        {
            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), instance, MethodInfo);
        }

        public Expression<TDelegate> ToDelegate<TDelegate>()
        {
            return System.Linq.Expressions.Expression.Lambda<TDelegate>(MethodCall, Lambda.Parameters);
        }
    }
}