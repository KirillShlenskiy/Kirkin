using System;
using System.Net;

using Xunit;

namespace Kirkin.Tests
{
    public class IsolationContextTests
    {
        [Fact]
        public void ServicePointManagerTest()
        {
            Assert.NotEqual(100, ServicePointManager.DefaultConnectionLimit);
            Assert.NotEqual(200, ServicePointManager.DefaultConnectionLimit);

            int initialConnectionLimit = ServicePointManager.DefaultConnectionLimit;

            ServicePointManagerProxy native = new ServicePointManagerProxy();

            using (IsolationContext isolated1 = new IsolationContext())
            using (IsolationContext isolated2 = new IsolationContext())
            {
                ServicePointManagerProxy proxy1 = isolated1.CreateInstance<ServicePointManagerProxy>();
                ServicePointManagerProxy proxy2 = isolated2.CreateInstance<ServicePointManagerProxy>();

                Assert.Equal(native.DefaultConnectionLimit, proxy1.DefaultConnectionLimit);
                Assert.Equal(native.DefaultConnectionLimit, proxy2.DefaultConnectionLimit);

                proxy1.DefaultConnectionLimit = 100;

                Assert.Equal(100, proxy1.DefaultConnectionLimit);
                Assert.Equal(initialConnectionLimit, proxy2.DefaultConnectionLimit);
                Assert.Equal(initialConnectionLimit, native.DefaultConnectionLimit);

                proxy2.DefaultConnectionLimit = 200;

                Assert.Equal(100, proxy1.DefaultConnectionLimit);
                Assert.Equal(200, proxy2.DefaultConnectionLimit);
                Assert.Equal(initialConnectionLimit, native.DefaultConnectionLimit);
            }

            Assert.Equal(initialConnectionLimit, ServicePointManager.DefaultConnectionLimit);
        }

        [Fact]
        public void ServicePointManagerUnloading()
        {
            ServicePointManagerProxy proxy;

            using (IsolationContext isolated = new IsolationContext())
            {
                proxy = isolated.CreateInstance<ServicePointManagerProxy>();
                proxy.DefaultConnectionLimit = 10;
            }

            Assert.Throws<AppDomainUnloadedException>(() => proxy.DefaultConnectionLimit);
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

        [Fact]
        public void BasicIsolation()
        {
            StaticState.Value = "zzz";

            using (IsolationContext isolated = new IsolationContext())
            {
                Manipulator manipulator = isolated.CreateInstance<Manipulator>();

                Assert.Null(manipulator.Value);

                manipulator.Value = "123";

                Assert.Equal("123", manipulator.Value);
                Assert.Equal("zzz", StaticState.Value);
            }
        }

        [Fact]
        public void CreateInstanceWithArgs()
        {
            using (IsolationContext isolated = new IsolationContext())
            {
                Manipulator manipulator = isolated.CreateInstance<Manipulator>("aaa");

                Assert.Equal("aaa", manipulator.Value);
            }
        }

        [Fact]
        public void IsolationContextSetupAndTeardownBenchmark()
        {
            for (int i = 0; i < 250; i++)
            {
                using (IsolationContext isolated = new IsolationContext())
                {
                }
            }
        }

        [Fact]
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