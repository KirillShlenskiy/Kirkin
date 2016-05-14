using System;
using System.Collections;
using System.Collections.Generic;

using Kirkin.ChangeTracking;

using Xunit;

namespace Kirkin.Tests.ChangeTracking
{
    internal static class PropertyValueComparerExtensions
    {
        public static PropertyValueEqualityComparer<T> WithStringComparer<T>(
            this PropertyValueEqualityComparer<T> propertyValueComparer,
            StringComparer stringComparer)
        {
            return new PropertyValueEqualityComparer<T>(propertyValueComparer.PropertyList, new Dictionary<Type, IEqualityComparer> {
                { typeof(string), stringComparer }
            });
        }
    }

    public class PropertyValueEqualityComparerTests
    {
        [Fact]
        public void EqualityComparerBenchmark()
        {
            IEqualityComparer<Dummy> comparer = PropertyValueEqualityComparer<Dummy>.Default;

            for (var i = 0; i < 1000000; i++)
            {
                var dummy1 = new Dummy {
                    ID = 1,
                    Value = "Text"
                };

                var dummy2 = new Dummy {
                    ID = 1,
                    Value = "Text"
                };

                Assert.True(comparer.Equals(dummy1, dummy2));
                Assert.False(!comparer.Equals(dummy1, dummy2));

                dummy2.Value = "zzz";

                Assert.False(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void EqualsBenchmark()
        {
            var comparer = PropertyValueEqualityComparer<Dummy>.Default;

            for (var i = 0; i < 1000000; i++)
            {
                var dummy1 = new Dummy {
                    ID = 1,
                    Value = "Text"
                };

                var dummy2 = new Dummy {
                    ID = 1,
                    Value = "Text"
                };

                Assert.True(comparer.Equals(dummy1, dummy2));
                Assert.False(!comparer.Equals(dummy1, dummy2));

                dummy2.Value = "zzz";

                Assert.False(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void EqualsNullity()
        {
            var comparer = PropertyValueEqualityComparer<Dummy>.Default;
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            Assert.True(comparer.Equals(dummy1, dummy2));
            Assert.True(comparer.Equals(null, null));
            Assert.False(comparer.Equals(dummy1, null));
            Assert.False(comparer.Equals(null, dummy2));
        }

        [Fact]
        public void EqualsStringComparer()
        {
            var comparer = PropertyValueEqualityComparer<Dummy>.Default;
            var dummy1 = new Dummy { ID = 1, Value = "zzz" };
            var dummy2 = new Dummy { ID = 1, Value = "ZZZ" };

            Assert.False(comparer.Equals(dummy1, dummy2));
            Assert.False(comparer.WithStringComparer(StringComparer.InvariantCulture).Equals(dummy1, dummy2));
            Assert.True(comparer.WithStringComparer(StringComparer.InvariantCultureIgnoreCase).Equals(dummy1, dummy2));
        }

        [Fact]
        [Obsolete("Suppressing GetHashCode warnings.")]
        public new void GetHashCode()
        {
            var comparer = PropertyValueEqualityComparer<Dummy>.Default;
            var dummy = new Dummy();

            dummy.ID = 1;

            Assert.Equal((17 * 23 + dummy.ID.GetHashCode()) * 23, comparer.GetHashCode(dummy));

            dummy.Value = "Zzz";

            Assert.Equal((17 * 23 + dummy.ID.GetHashCode()) * 23 + dummy.Value.GetHashCode(), comparer.GetHashCode(dummy));

            comparer = new PropertyValueEqualityComparer<Dummy>(comparer.PropertyList.Without(d => d.Value));

            Assert.Equal(17 * 23 + dummy.ID.GetHashCode(), comparer.GetHashCode(dummy));
        }

        [Fact]
        public void GetHashCodeStringComparer()
        {
            var comparer = PropertyValueEqualityComparer<Dummy>.Default;
            var dummy1 = new Dummy { ID = 1, Value = "zzz" };
            var dummy2 = new Dummy { ID = 1, Value = "ZZZ" };

            Assert.NotEqual(comparer.GetHashCode(dummy1), comparer.GetHashCode(dummy2));
            Assert.NotEqual(comparer.WithStringComparer(StringComparer.InvariantCulture).GetHashCode(dummy1), comparer.WithStringComparer(StringComparer.InvariantCulture).GetHashCode(dummy2));
            Assert.Equal(comparer.WithStringComparer(StringComparer.InvariantCultureIgnoreCase).GetHashCode(dummy1), comparer.WithStringComparer(StringComparer.InvariantCultureIgnoreCase).GetHashCode(dummy2));
        }

        [Fact]
        public void SameGetHashCodeWhenEquals()
        {
            var comparer = PropertyValueEqualityComparer<Dummy>.Default;
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            Assert.True(comparer.Equals(dummy1, dummy2));
            Assert.True(comparer.GetHashCode(dummy1) == comparer.GetHashCode(dummy2));

            dummy1.ID = 1;

            Assert.False(comparer.Equals(dummy1, dummy2));
            Assert.False(comparer.GetHashCode(dummy1) == comparer.GetHashCode(dummy2));

            dummy2.ID = 1;

            Assert.True(comparer.Equals(dummy1, dummy2));
            Assert.True(comparer.GetHashCode(dummy1) == comparer.GetHashCode(dummy2));
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}