using System.Collections.Immutable;

using Kirkin.Collections.Immutable;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class ImmutableArrayFactoryTests
    {
        [Test]
        public void BasicApi()
        {
            int[] integers = { 1, 2, 3 };
            ImmutableArray<int> immutable = ImmutableArrayFactory.WrapArray(integers);

            Assert.AreEqual(integers, immutable);
            Assert.False(immutable.IsDefault);

            ImmutableArray<int> def = new ImmutableArray<int>();

            Assert.True(def.IsDefault);
            Assert.AreEqual(0, ImmutableArray.Create<int>().Length);
        }

        [Test]
        public void NoCopy()
        {
            int[] integers = { 1, 2, 3 };
            ImmutableArray<int> immutable = ImmutableArrayFactory.WrapArray(integers);

            Assert.AreEqual(integers, immutable);

            integers[0] = 123;

            Assert.AreEqual(123, immutable[0]);
            Assert.AreEqual(integers, immutable);
        }

        [Test]
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