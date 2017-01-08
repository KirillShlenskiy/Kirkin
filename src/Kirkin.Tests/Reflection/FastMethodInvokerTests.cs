using System;
using System.Linq.Expressions;
using System.Reflection;

using Xunit;

namespace Kirkin.Tests.Reflection
{
    public class FastMethodInvokerTests
    {
        [Fact]
        public void MathMaxInvoke()
        {
            MethodInfo method = typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) });
            FastMethodInvoker invoker = new FastMethodInvoker(method);

            Assert.Equal(2, invoker.Invoke(null, 1, 2));
        }

        [Fact]
        public void TestActionNoArgs()
        {
            FastMethodInvoker method = CreateFastMethodInfo("ActionNoArgs");

            Assert.Null(method.Invoke(new Dummy(), null));
            Assert.Null(method.Invoke(new Dummy(), new int[0]));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, null));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, 42));
        }

        [Fact]
        public void TestActionOneArg()
        {
            FastMethodInvoker method = CreateFastMethodInfo("ActionOneArg");

            Assert.Null(method.Invoke(new Dummy(), 42));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, null));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, new object[0]));
        }

        [Fact]
        public void TestFuncNoArgs()
        {
            FastMethodInvoker method = CreateFastMethodInfo("FuncNoArgs");

            Assert.Equal(42, method.Invoke(new Dummy(), null));
            Assert.Equal(42, method.Invoke(new Dummy(), new int[0]));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, null));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, 42));
        }

        [Fact]
        public void TestFuncOneArg()
        {
            FastMethodInvoker method = CreateFastMethodInfo("FuncOneArg");

            Assert.Equal(42, method.Invoke(new Dummy(), 42));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, null));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, new object[0]));
        }

        private static FastMethodInvoker CreateFastMethodInfo(string methodName)
        {
            MethodInfo mi = typeof(Dummy).GetMethod(methodName);

            return new FastMethodInvoker(mi);
        }

        sealed class Dummy
        {
            public void ActionNoArgs()
            {
            }

            public void ActionOneArg(int i)
            {
            }

            public int FuncNoArgs()
            {
                return 42;
            }

            public int FuncOneArg(int i)
            {
                return i;
            }
        }

        /// <summary>
        /// Provides functionality similar to <see cref="System.Reflection.MethodInfo"/> Invoke method.
        /// Leverages compiled expression trees to improve on reflection's performance while maintaining a similar API.
        /// </summary>
        public sealed class FastMethodInvoker
        {
            private Func<object, object[], object> CompiledDelegate;

            /// <summary>
            /// Method invoked by this instance.
            /// </summary>
            public MethodInfo MethodInfo { get; }

            /// <summary>
            /// Creates a new <see cref="FastMethodInvoker"/> instance.
            /// </summary>
            public FastMethodInvoker(MethodInfo methodInfo)
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
                ParameterExpression argumentsExpression = Expression.Parameter(typeof(object[]), "arguments");
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
                ParameterExpression instanceExpression = Expression.Parameter(typeof(object), "instance");

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
}