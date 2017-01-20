using System.Collections.Generic;
using System.Linq;

using Kirkin.Collections.Generic;
using NUnit.Framework;

namespace Kirkin.Tests.Collections.Generic
{
    public class EnumerableUtilTests
    {
        [Test]
        public void Arrays()
        {
            var col1 = (IEnumerable<int>)new[] { 1, 2, 3 };
            var col2 = col1;

            EnumerableUtil.EnsureMaterialized(ref col2);

            Assert.AreSame(col1, col2);
        }

        [Test]
        public void Lists()
        {
            var col1 = (IEnumerable<int>)new List<int>(new[] { 1, 2, 3 });
            var col2 = col1;

            EnumerableUtil.EnsureMaterialized(ref col2);

            Assert.AreSame(col1, col2);
        }

        [Test]
        public void Materialize()
        {
            var col1 = Enumerable.Range(1, 3);
            var col2 = col1;

            EnumerableUtil.EnsureMaterialized(ref col2);

            Assert.AreNotSame(col1, col2);
        }
    }
}