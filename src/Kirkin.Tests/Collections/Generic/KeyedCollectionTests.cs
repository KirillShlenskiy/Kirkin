using Kirkin.Collections.Concurrent;
using Kirkin.Collections.Generic;

using NUnit.Framework;

namespace Kirkin.Tests.Collections.Generic
{
    public class KeyedCollectionTests
    {
        [Test]
        public void KeyedCollectionPerformance()
        {
            var table = new KeyedCollection<int, int>(i => i);

            for (int i = 0; i < 10000; i++)
            {
                table.Add(i);

                foreach (int value in table)
                {
                }
            }
        }

        [Test]
        public void ConcurrentKeyedCollectionPerformance()
        {
            var table = new ConcurrentKeyedCollection<int, int>(i => i);

            for (int i = 0; i < 10000; i++)
            {
                table.Add(i);

                foreach (int value in table)
                {
                }
            }
        }
    }
}