using System;

using Xunit;

namespace Kirkin.Tests
{
    public class IsolationContextTests
    {
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