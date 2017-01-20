using System;
using System.Net;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class IsolationContextTests
    {
        [Test]
        public void ServicePointManagerTest()
        {
            Assert.AreNotEqual(100, ServicePointManager.DefaultConnectionLimit);
            Assert.AreNotEqual(200, ServicePointManager.DefaultConnectionLimit);

            int initialConnectionLimit = ServicePointManager.DefaultConnectionLimit;

            ServicePointManagerProxy native = new ServicePointManagerProxy();

            using (IsolationContext isolated1 = new IsolationContext())
            using (IsolationContext isolated2 = new IsolationContext())
            {
                ServicePointManagerProxy proxy1 = isolated1.CreateInstance<ServicePointManagerProxy>();
                ServicePointManagerProxy proxy2 = isolated2.CreateInstance<ServicePointManagerProxy>();

                Assert.AreEqual(native.DefaultConnectionLimit, proxy1.DefaultConnectionLimit);
                Assert.AreEqual(native.DefaultConnectionLimit, proxy2.DefaultConnectionLimit);

                proxy1.DefaultConnectionLimit = 100;

                Assert.AreEqual(100, proxy1.DefaultConnectionLimit);
                Assert.AreEqual(initialConnectionLimit, proxy2.DefaultConnectionLimit);
                Assert.AreEqual(initialConnectionLimit, native.DefaultConnectionLimit);

                proxy2.DefaultConnectionLimit = 200;

                Assert.AreEqual(100, proxy1.DefaultConnectionLimit);
                Assert.AreEqual(200, proxy2.DefaultConnectionLimit);
                Assert.AreEqual(initialConnectionLimit, native.DefaultConnectionLimit);
            }

            Assert.AreEqual(initialConnectionLimit, ServicePointManager.DefaultConnectionLimit);
        }

        [Test]
        public void ServicePointManagerUnloading()
        {
            ServicePointManagerProxy proxy;

            using (IsolationContext isolated = new IsolationContext())
            {
                proxy = isolated.CreateInstance<ServicePointManagerProxy>();
                proxy.DefaultConnectionLimit = 10;
            }

            int limit;

            Assert.Throws<AppDomainUnloadedException>(() => limit = proxy.DefaultConnectionLimit);
            Assert.Throws<AppDomainUnloadedException>(() => proxy.DefaultConnectionLimit = 20);
        }

        sealed class ServicePointManagerProxy : MarshalByRefObject
        {
            public int DefaultConnectionLimit
            {
                get
                {
                    return ServicePointManager.DefaultConnectionLimit;
                }
                set
                {
                    ServicePointManager.DefaultConnectionLimit = value;
                }
            }
        }

        [Test]
        public void BasicIsolation()
        {
            StaticState.Value = "zzz";

            using (IsolationContext isolated = new IsolationContext())
            {
                Manipulator manipulator = isolated.CreateInstance<Manipulator>();

                Assert.Null(manipulator.Value);

                manipulator.Value = "123";

                Assert.AreEqual("123", manipulator.Value);
                Assert.AreEqual("zzz", StaticState.Value);
            }
        }

        [Test]
        public void CreateInstanceWithArgs()
        {
            using (IsolationContext isolated = new IsolationContext())
            {
                Manipulator manipulator = isolated.CreateInstance<Manipulator>("aaa");

                Assert.AreEqual("aaa", manipulator.Value);
            }
        }

        [Test]
        public void IsolationContextSetupAndTeardownBenchmark()
        {
            for (int i = 0; i < 250; i++)
            {
                using (IsolationContext isolated = new IsolationContext())
                {
                }
            }
        }

        [Test]
        public void IsolationContextCreateInstanceBenchmark()
        {
            using (IsolationContext isolated = new IsolationContext())
            {
                for (int i = 0; i < 250; i++) {
                    isolated.CreateInstance<Manipulator>();
                }
            }
        }

        sealed class Manipulator : MarshalByRefObject
        {
            public Manipulator()
            {
            }

            public Manipulator(string initialValue)
            {
                Value = initialValue;
            }

            public string Value
            {
                get
                {
                    return StaticState.Value;
                }
                set
                {
                    StaticState.Value = value;
                }
            }
        }

        static class StaticState
        {
            public static string Value;
        }
    }
}