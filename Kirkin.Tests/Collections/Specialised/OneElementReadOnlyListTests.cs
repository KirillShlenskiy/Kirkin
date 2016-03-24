using System.Collections.Generic;

using Kirkin.Collections.Specialised;

using Xunit;

namespace Kirkin.Tests.Collections.Specialised
{
    public class OneElementReadOnlyListTests
    {
        [Fact]
        public void EnumerationDirect()
        {
            OneElementReadOnlyList<int> collection = new OneElementReadOnlyList<int>(123);
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
            IEnumerable<int> collection = new OneElementReadOnlyList<int>(123);
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
            OneElementReadOnlyList<int> collection = new OneElementReadOnlyList<int>(123);

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
            IEnumerable<int> collection = new OneElementReadOnlyList<int>(123);

            for (int i = 0; i < 1000000; i++)
            {
                foreach (int item in collection)
                {
                }
            }
        }
    }
}