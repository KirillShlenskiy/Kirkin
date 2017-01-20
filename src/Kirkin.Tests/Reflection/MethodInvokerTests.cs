using System;
using System.Reflection;

using Kirkin.Reflection;

using NUnit.Framework;

namespace Kirkin.Tests.Reflection
{
    public class MethodInvokerTests
    {
        [Test]
        public void MathMaxInvoke()
        {
            MethodInfo method = typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) });
            MethodInvoker invoker = new MethodInvoker(method);

            Assert.AreEqual(2, invoker.Invoke(null, 1, 2));
        }

        [Test]
        public void TestActionNoArgs()
        {
            MethodInvoker invoker = CreateFastMethodInfo("ActionNoArgs");

            Assert.Null(invoker.Invoke(new Dummy(), null));
            Assert.Null(invoker.Invoke(new Dummy(), new int[0]));
            Assert.Throws<NullReferenceException>(() => invoker.Invoke(null, null));
            Assert.Throws<NullReferenceException>(() => invoker.Invoke(null, 42));
        }

        [Test]
        public void TestActionOneArg()
        {
            MethodInvoker invoker = CreateFastMethodInfo("ActionOneArg");

            Assert.Null(invoker.Invoke(new Dummy(), 42));
            Assert.Throws<NullReferenceException>(() => invoker.Invoke(null, null));
            Assert.Throws<IndexOutOfRangeException>(() => invoker.Invoke(null, new object[0]));
        }

        [Test]
        public void TestFuncNoArgs()
        {
            MethodInvoker invoker = CreateFastMethodInfo("FuncNoArgs");

            Assert.AreEqual(42, invoker.Invoke(new Dummy(), null));
            Assert.AreEqual(42, invoker.Invoke(new Dummy(), new int[0]));
            Assert.Throws<NullReferenceException>(() => invoker.Invoke(null, null));
            Assert.Throws<NullReferenceException>(() => invoker.Invoke(null, 42));
        }

        [Test]
        public void TestFuncOneArg()
        {
            MethodInvoker invoker = CreateFastMethodInfo("FuncOneArg");

            Assert.AreEqual(42, invoker.Invoke(new Dummy(), 42));
            Assert.Throws<NullReferenceException>(() => invoker.Invoke(null, null));
            Assert.Throws<IndexOutOfRangeException>(() => invoker.Invoke(null, new object[0]));
        }

        private static MethodInvoker CreateFastMethodInfo(string methodName)
        {
            MethodInfo mi = typeof(Dummy).GetMethod(methodName);

            return new MethodInvoker(mi);
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
    }
}