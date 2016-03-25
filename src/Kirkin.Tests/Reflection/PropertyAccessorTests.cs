using System;
using System.Reflection;

using Kirkin.Reflection;

using Xunit;

namespace Kirkin.Tests.Reflection
{
    public class PropertyAccessorTests
    {
        private const int BENCHMARK_ITERATIONS = 1000000;

        [Fact]
        public void PropertyAccessorBenchmark()
        {
            var fastValue = new PropertyAccessor<Dummy, string>(typeof(Dummy).GetProperty("Value"));

            for (var i = 0; i < BENCHMARK_ITERATIONS; i++)
            {
                var dummy = new Dummy();

                Assert.True(fastValue.GetValue(dummy) == null);

                fastValue.SetValue(dummy, "Whatever");

                Assert.Equal("Whatever", fastValue.GetValue(dummy));
                Assert.Equal("Whatever", dummy.Value);
            }
        }

        [Fact]
        public void DowncastGenericPropertyAccessorBenchmark()
        {
            var fastValue = (IPropertyAccessor)new PropertyAccessor<Dummy, string>(typeof(Dummy).GetProperty("Value"));

            for (var i = 0; i < BENCHMARK_ITERATIONS; i++)
            {
                var dummy = new Dummy();

                Assert.True(fastValue.GetValue(dummy) == null);

                fastValue.SetValue(dummy, "Whatever");

                Assert.Equal("Whatever", fastValue.GetValue(dummy));
                Assert.Equal("Whatever", dummy.Value);
            }
        }

        [Fact(Skip = "Redo")]
        public void NonGenericPropertyAccessorBenchmark()
        {
            //var fastValue = new PropertyAccessor(typeof(Dummy).GetProperty("Value"));

            //for (var i = 0; i < BENCHMARK_ITERATIONS; i++)
            //{
            //    var dummy = new Dummy();

            //    Assert.True(fastValue.GetValue(dummy) == null);

            //    fastValue.SetValue(dummy, "Whatever");

            //    Assert.Equal("Whatever", fastValue.GetValue(dummy));
            //    Assert.Equal("Whatever", dummy.Value);
            //}
        }

        [Fact]
        public void DirectAccessBenchmark()
        {
            for (var i = 0; i < BENCHMARK_ITERATIONS; i++)
            {
                var dummy = new Dummy();

                Assert.True(dummy.Value == null);

                dummy.Value = "Whatever";

                Assert.Equal("Whatever", dummy.Value);
                Assert.Equal("Whatever", dummy.Value);
            }
        }

        [Fact]
        public void ReflectionBenchmark()
        {
            var valueProperty = typeof(Dummy).GetProperty("Value");

            for (var i = 0; i < BENCHMARK_ITERATIONS; i++)
            {
                var dummy = new Dummy();

                Assert.True(valueProperty.GetValue(dummy, null) == null);

                valueProperty.SetValue(dummy, "Whatever", null);

                Assert.Equal("Whatever", valueProperty.GetValue(dummy, null));
                Assert.Equal("Whatever", dummy.Value);
            }
        }

        [Fact]
        public void PublicPropertyTest()
        {
            var id = new PropertyAccessor<Dummy, int>(typeof(Dummy).GetProperty("ID"));
            var dummy = new Dummy();

            id.SetValue(dummy, 42);

            Assert.Equal(42, id.GetValue(dummy));
        }

        [Fact]
        public void PrivatePropertyTest()
        {
            var prop = new PropertyAccessor<Dummy, int>(
                typeof(Dummy).GetProperty("PrivateProp", BindingFlags.Instance | BindingFlags.NonPublic)
            );

            var dummy = new Dummy();

            prop.SetValue(dummy, 42);

            Assert.Equal(42, prop.GetValue(dummy));
        }

        [Fact]
        public void PrivateSetterPropertyTest()
        {
            var prop = new PropertyAccessor<Dummy, int>(typeof(Dummy).GetProperty("PrivateSetterProp"));
            var dummy = new Dummy();

            Assert.Throws<InvalidOperationException>(() => prop.SetValue(dummy, 42));
        }

        [Fact(Skip = "Redo")]
        public void NonGenericTest()
        {
            //var prop = new PropertyAccessor(typeof(Dummy).GetProperty("ID"));
            //var dummy = new Dummy();

            //prop.SetValue(dummy, 42);

            //Assert.Equal(42, prop.GetValue(dummy));
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
            private int PrivateProp { get; set; }

            private int _privateSetterProp = 0;

            public int PrivateSetterProp { get { return _privateSetterProp; } }
            public static int StaticValue { get; set; }
        }

        [Fact]
        public void IsStaticBenchmark()
        {
            var idProperty = typeof(Dummy).GetProperty("ID");
            var valueProperty = typeof(Dummy).GetProperty("StaticValue");

            for (var i = 0; i < 10000; i++)
            {
                Assert.False(PropertyAccessor<object, object>.IsStatic(idProperty));
                Assert.True(PropertyAccessor<object, object>.IsStatic(valueProperty));
            }
        }
    }
}
