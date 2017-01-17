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

            using (IsolationContext context1 = new IsolationContext())
            using (IsolationContext context2 = new IsolationContext())
            {
                ServicePointManagerProxy isolated1 = context1.CreateInstance<ServicePointManagerProxy>();
                ServicePointManagerProxy isolated2 = context2.CreateInstance<ServicePointManagerProxy>();

                Assert.Equal(native.DefaultConnectionLimit, isolated1.DefaultConnectionLimit);
                Assert.Equal(native.DefaultConnectionLimit, isolated2.DefaultConnectionLimit);

                isolated1.DefaultConnectionLimit = 100;

                Assert.Equal(100, isolated1.DefaultConnectionLimit);
                Assert.Equal(initialConnectionLimit, isolated2.DefaultConnectionLimit);
                Assert.Equal(initialConnectionLimit, native.DefaultConnectionLimit);

                isolated2.DefaultConnectionLimit = 200;

                Assert.Equal(100, isolated1.DefaultConnectionLimit);
                Assert.Equal(200, isolated2.DefaultConnectionLimit);
                Assert.Equal(initialConnectionLimit, native.DefaultConnectionLimit);
            }

            Assert.Equal(initialConnectionLimit, ServicePointManager.DefaultConnectionLimit);
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

            using (IsolationContext context = new IsolationContext())
            {
                Manipulator manipulator = context.CreateInstance<Manipulator>();

                Assert.Null(manipulator.Value);

                manipulator.Value = "123";

                Assert.Equal("123", manipulator.Value);
                Assert.Equal("zzz", StaticState.Value);
            }
        }

        [Fact]
        public void CreateInstanceWithArgs()
        {
            using (IsolationContext context = new IsolationContext())
            {
                Manipulator manipulator = context.CreateInstance<Manipulator>("aaa");

                Assert.Equal("aaa", manipulator.Value);
            }
        }

        [Fact]
        public void IsolationContextSetupAndTeardownBenchmark()
        {
            for (int i = 0; i < 250; i++)
            {
                using (IsolationContext context = new IsolationContext())
                {
                }
            }
        }

        [Fact]
        public void IsolationContextCreateInstanceBenchmark()
        {
            using (IsolationContext context = new IsolationContext())
            {
                for (int i = 0; i < 250; i++) {
                    context.CreateInstance<Manipulator>();
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