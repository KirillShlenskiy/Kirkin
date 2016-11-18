using Kirkin.ChangeTracking;
using Kirkin.Mapping;
using Kirkin.Reflection;

using Xunit;

namespace Kirkin.Tests.Reflection
{
    public class PropertyListTests
    {
        [Fact]
        public void CopyBenchmarkLarge()
        {
            var comparer = new PropertyValueEqualityComparer<Dummy>(PropertyList<Dummy>.Default);
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
        public void CopyBenchmarkMedium()
        {
            var comparer = new PropertyValueEqualityComparer<Dummy>(PropertyList<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            for (var i = 0; i < 10000; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                Mapper.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkSmall()
        {
            var comparer = new PropertyValueEqualityComparer<Dummy>(PropertyList<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            for (var i = 0; i < 10; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                Mapper.Map(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkMapperLarge()
        {
            var comparer = new PropertyValueEqualityComparer<Dummy>(PropertyList<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();
            var mapper = new MapperBuilder<Dummy, Dummy>().BuildMapper();

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
            var comparer = new PropertyValueEqualityComparer<Dummy>(PropertyList<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();
            var mapper = new MapperBuilder<Dummy, Dummy>().BuildMapper();

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
            AutoMapper.Mapper.Initialize(config => config.CreateMap<Dummy, Dummy>());

            var comparer = new PropertyValueEqualityComparer<Dummy>(PropertyList<Dummy>.Default);
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
            var comparer = new PropertyValueEqualityComparer<Dummy>(PropertyList<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();

            for (var i = 0; i < 1000000; i++)
            {
                dummy1.ID = i;
                dummy1.Value = "Text " + i;

                Assert.False(comparer.Equals(dummy1, dummy2));
                Mapper.MapStrict(dummy1, dummy2);
                Assert.True(comparer.Equals(dummy1, dummy2));
            }
        }

        [Fact]
        public void CopyBenchmarkMapperSmall()
        {
            var comparer = new PropertyValueEqualityComparer<Dummy>(PropertyList<Dummy>.Default);
            var dummy1 = new Dummy();
            var dummy2 = new Dummy();
            var mapper = new MapperBuilder<Dummy, Dummy>().BuildMapper();

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
        public new void ToString()
        {
            var propertyList = PropertyList<Dummy>.Default;

            var dummy = new Dummy {
                ID = 1,
                Value = "Text"
            };

            for (int i = 0; i < 100000; i++) {
                Assert.Equal("Dummy { ID = 1, Value = Text }", propertyList.ToString(dummy));
            }
        }

        [Fact]
        public void MultiExclude()
        {
            var propertyList = PropertyList<MultiDummy>.Default
                .Without(d => d.ID)
                .Without(d => d.Ignored2);

            Assert.Equal(2, propertyList.PropertyAccessors.Length);
        }

        [Fact]
        public void ExcludeAndReInclude()
        {
            var propertyList = PropertyList<MultiDummy>.Default.Without(d => d.ID);

            Assert.Equal(3, propertyList.PropertyAccessors.Length);

            propertyList = propertyList.Including(d => d.ID);

            Assert.Equal(4, propertyList.PropertyAccessors.Length);

            propertyList = propertyList.Including(d => d.ID);

            Assert.Equal(4, propertyList.PropertyAccessors.Length);
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        private class MultiDummy : Dummy
        {
            public int Ignored1 { get; set; }
            public string Ignored2 { get; set; }
        }
    }
}