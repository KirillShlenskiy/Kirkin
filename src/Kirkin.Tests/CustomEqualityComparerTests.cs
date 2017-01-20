using System;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class CustomEqualityComparerTests
    {
        [Test]
        public void Equals()
        {
            var comparer = new CustomEqualityComparer<int>((x, y) => true);

            Assert.True(comparer.Equals(1, 2));
        }

        [Test]
        public void EqualsAndGetHashCode()
        {
            var comparer = new CustomEqualityComparer<int>((x, y) => true, i => 321);

            Assert.True(comparer.Equals(1, 2));
            Assert.True(comparer.SupportsGetHashCode);
            Assert.True(comparer.GetHashCode(123) == 321);
        }

        [Test]
        public void NoGetHashCode()
        {
            var comparer = new CustomEqualityComparer<int>((x, y) => true);

            Assert.False(comparer.SupportsGetHashCode);

            // Will throw a NotSupportedException.
            Assert.Throws<NotSupportedException>(() => comparer.GetHashCode(123));
        }
    }
}