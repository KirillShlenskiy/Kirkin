using System.Collections.Immutable;

using Kirkin.Collections.Immutable;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    public class ImmutableArrayFactoryTests
    {
        [Fact]
        public void BasicApi()
        {
            int[] integers = { 1, 2, 3 };
            ImmutableArray<int> immutable = ImmutableArrayFactory.WrapArray(integers);

            Assert.Equal(integers, immutable);
            Assert.False(immutable.IsDefault);

            ImmutableArray<int> def = new ImmutableArray<int>();

            Assert.True(def.IsDefault);
            Assert.Equal(0, ImmutableArray.Create<int>().Length);
        }

        [Fact]
        public void WrapWithReflectionPerf()
        {
            int[] integers = { 1, 2, 3 };
            ImmutableArray<int> immutable;

            for (int i = 0; i < 250000; i++) {
                immutable = ImmutableArrayFactory.WrapArray(integers);
            }
        }
    }
}