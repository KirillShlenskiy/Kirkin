using System.Collections.Generic;

using Kirkin.Collections.Generic.Enumerators;

using NUnit.Framework;

namespace Kirkin.Tests.Collections.Generic
{
    public class StructEnumeratorFinderTests
    {
        [Test]
        public void StructEnumeratorResolverForListSucceeds()
        {
            Assert.NotNull(StructEnumeratorFinder.FindValueTypeGetEnumeratorMethod(typeof(List<int>)));
        }

        [Test]
        public void StructEnumeratorResolverForEnumerableFails()
        {
            Assert.Null(StructEnumeratorFinder.FindValueTypeGetEnumeratorMethod(typeof(int[])));
        }
    }
}