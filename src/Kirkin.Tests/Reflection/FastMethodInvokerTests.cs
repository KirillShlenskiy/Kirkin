using System;
using System.Linq.Expressions;
using System.Reflection;

using Xunit;

namespace Kirkin.Tests.Reflection
{
    public class FastMethodInvokerTests
    {
        [Fact]
        public void TestActionNoArgs()
        {
            FastMethodInvoker method = CreateFastMethodInfo("ActionNoArgs");

            Assert.Null(method.Invoke(new Dummy(), null));
            Assert.Null(method.Invoke(new Dummy(), new int[0]));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, null));
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, new object[] { 1 }));
        }

        [Fact]
        public void TestActionOneArg()
        {
            FastMethodInvoker method = CreateFastMethodInfo("ActionOneArg");

            Assert.Null(method.Invoke(new Dummy(), new object[] { 42 }));
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
            Assert.ThrowsAny<Exception>(() => method.Invoke(null, new object[] { 1 }));
        }

        [Fact]
        public void TestFuncOneArg()
        {
            FastMethodInvoker method = CreateFastMethodInfo("FuncOneArg");

            Assert.Equal(42, method.Invoke(new Dummy(), new object[] { 42 }));
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
        /// </summary>
        public sealed class FastMethodInvoker
        {
            /// <summary>
            /// Method invoked by this instance.
            /// </summary>
            public MethodInfo MethodInfo { get; }
            private readonly Func<object, object[], object> CompiledDelegate;

            /// <summary>
            /// Creates a new <see cref="FastMethodInvoker"/> instance.
            /// </summary>
            public FastMethodInvoker(MethodInfo methodInfo)
            {
                if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

                MethodInfo = methodInfo;

                ParameterExpression argumentsExpression = Expression.Parameter(typeof(object[]), "arguments");
                ParameterInfo[] parameters = methodInfo.GetParameters();
                Expression[] argumentExpressions = new Expression[parameters.Length];

                for (int i = 0; i < parameters.Length; ++i)
                {
                    argumentExpressions[i] = Expression.Convert(
                        Expression.ArrayIndex(argumentsExpression, Expression.Constant(i)),
                        parameters[i].ParameterType
                    );
                }

                ParameterExpression instanceExpression = null;
                MethodCallExpression callExpression = null;

                if (methodInfo.IsStatic)
                {
                    callExpression = Expression.Call(null, methodInfo, argumentExpressions);
                }
                else
                {
                    instanceExpression = Expression.Parameter(typeof(object), "instance");
                    callExpression = Expression.Call(Expression.Convert(instanceExpression, methodInfo.ReflectedType), methodInfo, argumentExpressions);
                }

                if (methodInfo.ReturnType == typeof(void))
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

            /// <summary>
            /// Invokes the target method on the given object instance (pass null if the method is static).
            /// </summary>
            public object Invoke(object instance, params object[] arguments)
            {
                return CompiledDelegate(instance, arguments);
            }
        }
    }
}