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

        sealed class Manipulator : MarshalByRefObject
        {
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