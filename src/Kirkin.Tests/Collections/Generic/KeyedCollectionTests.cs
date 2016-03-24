using Kirkin.Collections.Concurrent;
using Kirkin.Collections.Generic;

using Xunit;

namespace Kirkin.Tests.Collections.Generic
{
    public class KeyedCollectionTests
    {
        [Fact]
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

        [Fact]
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