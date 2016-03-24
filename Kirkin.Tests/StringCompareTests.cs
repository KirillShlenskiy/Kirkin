using System;

using Xunit;

namespace Kirkin.Tests
{
    public class StringCompareBenchmarks
    {
        [Fact]
        public void ComparisonInvariantCulture()
        {
            Comparison(StringComparison.InvariantCulture);
        }

        [Fact]
        public void ComparisonInvariantCultureIgnoreCase()
        {
            Comparison(StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void ComparisonOrdinal()
        {
            Comparison(StringComparison.Ordinal);
        }

        [Fact]
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

        [Fact]
        public void ComparerInvariantCulture()
        {
            Comparer(StringComparer.InvariantCulture);
        }

        [Fact]
        public void ComparerInvariantCultureIgnoreCase()
        {
            Comparer(StringComparer.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void ComparerOrdinal()
        {
            Comparer(StringComparer.Ordinal);
        }

        [Fact]
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