using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Castle.DynamicProxy;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    internal static class ProxyGeneratorExtensions
    {
        //public static TInterface CreateVoid
    }

    public class CastleCodegenTests
    {
        [Fact]
        public void DynamicInterface()
        {
            ProxyGenerator generator = new ProxyGenerator();
            IAction<string> action = generator.CreateInterfaceProxyWithoutTarget<IAction<string>>(Interceptors.VoidInterceptor<string>(s => Debug.Print(s)));

            action.Invoke("hello");

            Debug.Print("Done.");
        }

        public interface IAction<T>
        {
            void Invoke(T arg);
        }

        public interface IFunc<T>
        {
            T Invoke();
        }

        sealed class Interceptors
        {
            public static IInterceptor VoidInterceptor(Action action)
            {
                return new Interceptor(action);
            }

            public static IInterceptor VoidInterceptor<TArg>(Action<TArg> action)
            {
                return new Interceptor(action);
            }

            public static IInterceptor FuncInterceptor<TReturn>(Func<TReturn> func)
            {
                return new Interceptor(func);
            }

            public static IInterceptor FuncInterceptor<TArg, TReturn>(Func<TArg, TReturn> func)
            {
                return new Interceptor(func);
            }

            sealed class Interceptor : IInterceptor
            {
                private readonly Delegate Delegate;

                internal Interceptor(Delegate @delegate)
                {
                    Delegate = @delegate;
                }

                public void Intercept(IInvocation invocation)
                {
                    invocation.ReturnValue = Delegate.DynamicInvoke(invocation.Arguments);
                }
            }
        }
    }
}