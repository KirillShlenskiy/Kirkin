using System.Linq;

using Kirkin.Collections.Filters;

using NUnit.Framework;

namespace Kirkin.Tests.Collections.Filters
{
    public class CollectionFilterTests
    {
        [Test]
        public void FilterByProjection()
        {
            string[] input = { "1", "2", "3", "4", "5" };
            ICollectionFilter<int> evensFilter = CollectionFilter.FromPredicate<int>(i => i % 2 == 0);

            string[] result = evensFilter
                .FilterByProjection(input, i => int.Parse(i))
                .ToArray();

            Assert.AreEqual(new[] { "2", "4" }, result);
        }
    }
}