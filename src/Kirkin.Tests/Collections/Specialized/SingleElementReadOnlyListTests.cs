using System.Collections.Generic;

using Kirkin.Collections.Specialized;

using NUnit.Framework;

namespace Kirkin.Tests.Collections.Specialized
{
    public class SingleElementReadOnlyListTests
    {
        [Test]
        public void EnumerationDirect()
        {
            SingleElementReadOnlyList<int> collection = new SingleElementReadOnlyList<int>(123);
            int i = 0;

            foreach (int item in collection)
            {
                Assert.AreEqual(123, item);

                i++;
            }

            Assert.AreEqual(1, i);
        }

        [Test]
        public void EnumerationViaInterface()
        {
            IEnumerable<int> collection = new SingleElementReadOnlyList<int>(123);
            int i = 0;

            foreach (int item in collection)
            {
                Assert.AreEqual(123, item);

                i++;
            }

            Assert.AreEqual(1, i);
        }

        [Test]
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

        [Test]
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