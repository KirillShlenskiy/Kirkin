using System;
using System.Collections.Generic;

using Kirkin.Mapping;

using Xunit;

namespace Kirkin.Tests
{
    public class TypeMappingTests
    {
        [Fact]
        public void CopyBenchmarkLarge()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>(TypeMapping<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            for (var i = 0; i < 1000000; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                TypeMapping.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkMedium()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>(TypeMapping<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            for (var i = 0; i < 10000; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                TypeMapping.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkSmall()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>(TypeMapping<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            for (var i = 0; i < 10; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                TypeMapping.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkMapperLarge()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>(TypeMapping<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();
            var mapper = Mapper.Create<Dummy, Dummy>();

            for (var i = 0; i < 1000000; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                mapper.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkMapperMedium()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>(TypeMapping<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();
            var mapper = Mapper.Create<Dummy, Dummy>();

            for (var i = 0; i < 10000; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                mapper.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkAutoMapperLargeDefaultMapper()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>(TypeMapping<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            for (var i = 0; i < 1000000; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                AutoMapper.Mapper.Map(dummy1, dummy2);
                //Assert.True(comparer.Equals(dummy1, dummy2)); // AutoMapper doesn't do the right thing here.
            }
        }

        [Fact]
        public void CopyBenchmarkMapperLargeDefaultMapper()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>(TypeMapping<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            for (var i = 0; i < 1000000; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                Mapper.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkMapperSmall()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>(TypeMapping<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();
            var mapper = Mapper.Create<Dummy, Dummy>();

            for (var i = 0; i < 10; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                mapper.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void EqualityComparerBenchmark()
        {
            IEqualityComparer<Dummy> comparer = new TypeMappingEqualityComparer<Dummy>();

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
            var comparer = new TypeMappingEqualityComparer<Dummy>();

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
            var comparer = new TypeMappingEqualityComparer<Dummy>();
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
            var comparer = new TypeMappingEqualityComparer<Dummy>();
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
            var comparer = new TypeMappingEqualityComparer<Dummy>();
            var dummy = new Dummy();

            dummy.ID = 1;

            Assert.Equal((17 * 23 + dummy.ID.GetHashCode()) * 23, comparer.GetHashCode(dummy));

            dummy.Value = "Zzz";

            Assert.Equal((17 * 23 + dummy.ID.GetHashCode()) * 23 + dummy.Value.GetHashCode(), comparer.GetHashCode(dummy));

            comparer = new TypeMappingEqualityComparer<Dummy>(comparer.TypeMapping.Without(d => d.Value));

            Assert.Equal(17 * 23 + dummy.ID.GetHashCode(), comparer.GetHashCode(dummy));
        }

        [Fact]
        public void GetHashCodeStringComparer()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>();
            var dummy1 = new Dummy { ID = 1, Value = "zzz" };
            var dummy2 = new Dummy { ID = 1, Value = "ZZZ" };

            Assert.NotEqual(comparer.GetHashCode(dummy1), comparer.GetHashCode(dummy2));
            Assert.NotEqual(comparer.WithStringComparer(StringComparer.InvariantCulture).GetHashCode(dummy1), comparer.WithStringComparer(StringComparer.InvariantCulture).GetHashCode(dummy2));
            Assert.Equal(comparer.WithStringComparer(StringComparer.InvariantCultureIgnoreCase).GetHashCode(dummy1), comparer.WithStringComparer(StringComparer.InvariantCultureIgnoreCase).GetHashCode(dummy2));
        }

        [Fact]
        public void SameGetHashCodeWhenEquals()
        {
            var comparer = new TypeMappingEqualityComparer<Dummy>();
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

        [Fact]
        public new void ToString()
        {
            var mapping = TypeMapping<Dummy>.Default;

            var dummy = new Dummy {
                ID = 1,
                Value = "Text"
            };

            for (int i = 0; i < 100000; i++) {
            Assert.Equal("Dummy { ID = 1, Value = Text }", mapping.ToString(dummy));
            }
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        [Fact]
        public void MultiExclude()
        {
            var mapping = TypeMapping<MultiDummy>.Default
                .Without(d => d.ID)
                .Without(d => d.Ignored2);

            Assert.Equal(2, mapping.PropertyAccessors.Length);
        }

        private class MultiDummy : Dummy
        {
            public int Ignored1 { get; set; }
            public string Ignored2 { get; set; }
        }
    }
}