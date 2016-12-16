using System.Collections.Generic;

using Kirkin.Collections.Specialized;

using Xunit;

namespace Kirkin.Tests.Collections.Specialized
{
    public class SingleElementReadOnlyListTests
    {
        [Fact]
        public void EnumerationDirect()
        {
            SingleElementReadOnlyList<int> collection = new SingleElementReadOnlyList<int>(123);
            int i = 0;

            foreach (int item in collection)
            {
                Assert.Equal(123, item);

                i++;
            }

            Assert.Equal(1, i);
        }

        [Fact]
        public void EnumerationViaInterface()
        {
            IEnumerable<int> collection = new SingleElementReadOnlyList<int>(123);
            int i = 0;

            foreach (int item in collection)
            {
                Assert.Equal(123, item);

                i++;
            }

            Assert.Equal(1, i);
        }

        [Fact]
        public void PerfDirect()
        {
            SingleElementReadOnlyList<int> collection = new SingleElementReadOnlyList<int>(123);

            for (int i = 0; i < 1000000; i++)
            {
                foreach (int item in collection)
                {
                }
            }
        }

        [Fact]
        public void PerfInterface()
        {
            IEnumerable<int> collection = new SingleElementReadOnlyList<int>(123);

            for (int i = 0; i < 1000000; i++)
            {
                foreach (int item in collection)
                {
                }
            }
        }
    }
}