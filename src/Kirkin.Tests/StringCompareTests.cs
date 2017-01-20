using System;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class StringCompareBenchmarks
    {
        [Test]
        public void ComparisonInvariantCulture()
        {
            Comparison(StringComparison.InvariantCulture);
        }

        [Test]
        public void ComparisonInvariantCultureIgnoreCase()
        {
            Comparison(StringComparison.InvariantCultureIgnoreCase);
        }

        [Test]
        public void ComparisonOrdinal()
        {
            Comparison(StringComparison.Ordinal);
        }

        [Test]
        public void ComparisonOrdinalIgnoreCase()
        {
            Comparison(StringComparison.OrdinalIgnoreCase);
        }

        private void Comparison(StringComparison sc)
        {
            string s1 = "Zzz";
            string s2 = "zzz";

            for (int i = 0; i < 10000000; i++)
            {
                var o = string.Equals(s1, s2, sc);
            }
        }

        [Test]
        public void ComparerInvariantCulture()
        {
            Comparer(StringComparer.InvariantCulture);
        }

        [Test]
        public void ComparerInvariantCultureIgnoreCase()
        {
            Comparer(StringComparer.InvariantCultureIgnoreCase);
        }

        [Test]
        public void ComparerOrdinal()
        {
            Comparer(StringComparer.Ordinal);
        }

        [Test]
        public void ComparerOrdinalIgnoreCase()
        {
            Comparer(StringComparer.OrdinalIgnoreCase);
        }

        private void Comparer(StringComparer comparer)
        {
            string s1 = "Zzz";
            string s2 = "zzz";

            for (int i = 0; i < 10000000; i++)
            {
                var o = comparer.Equals(s1, s2);
            }
        }
    }
}