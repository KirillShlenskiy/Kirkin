using System.Collections.Generic;
using System.Linq;

using Kirkin.Collections.Generic;
using Xunit;

namespace Kirkin.Tests.Collections.Generic
{
    public class EnumerableUtilTests
    {
        [Fact]
        public void Arrays()
        {
            var col1 = (IEnumerable<int>)new[] { 1, 2, 3 };
            var col2 = col1;

            EnumerableUtil.EnsureMaterialised(ref col2);

            Assert.Same(col1, col2);
        }

        [Fact]
        public void Lists()
        {
            var col1 = (IEnumerable<int>)new List<int>(new[] { 1, 2, 3 });
            var col2 = col1;

            EnumerableUtil.EnsureMaterialised(ref col2);

            Assert.Same(col1, col2);
        }

        [Fact]
        public void Materialise()
        {
            var col1 = Enumerable.Range(1, 3);
            var col2 = col1;

            EnumerableUtil.EnsureMaterialised(ref col2);

            Assert.NotSame(col1, col2);
        }
    }
}