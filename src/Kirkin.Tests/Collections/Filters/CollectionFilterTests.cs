using System.Linq;

using Kirkin.Collections.Filters;

using Xunit;

namespace Kirkin.Tests.Collections.Filters
{
    public class CollectionFilterTests
    {
        [Fact]
        public void FilterByProjection()
        {
            string[] input = { "1", "2", "3", "4", "5" };
            ICollectionFilter<int> evensFilter = CollectionFilter.FromPredicate<int>(i => i % 2 == 0);

            string[] result = evensFilter
                .FilterByProjection(input, i => int.Parse(i))
                .ToArray();

            Assert.Equal(new[] { "2", "4" }, result);
        }
    }
}