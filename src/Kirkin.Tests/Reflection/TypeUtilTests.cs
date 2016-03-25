using System;
using System.Collections.Generic;
using System.Linq;

using Kirkin.Reflection;
using Kirkin.Utilities;

using Xunit;

namespace Kirkin.Tests.Reflection
{
    public class TypeUtilTests
    {
        [Fact]
        public void PropertyByExpression()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = TypeUtil<Dummy>.Property(d => d.ID);

                idProp.SetValue(dummy, 100);

                Assert.Equal(100, idProp.GetValue(dummy));
                Assert.Equal(100, dummy.ID);
            }
        }

        [Fact]
        public void PropertyByName()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = TypeUtil<Dummy>.Property<int>("ID");

                idProp.SetValue(dummy, 100);

                Assert.Equal(100, idProp.GetValue(dummy));
                Assert.Equal(100, dummy.ID);
            }
        }

        [Fact]
        public void NonGenericPropertyByName()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = TypeUtil<Dummy>.Property("ID");

                idProp.SetValue(dummy, 100);

                Assert.Equal(100, idProp.GetValue(dummy));
                Assert.Equal(100, dummy.ID);
            }
        }

        [Fact]
        public void NonGenericTypeUtilNonGenericPropertyByName()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = TypeUtil.Property(typeof(Dummy), "ID");

                idProp.SetValue(dummy, 100);

                Assert.Equal(100, idProp.GetValue(dummy));
                Assert.Equal(100, dummy.ID);
            }
        }

        [Fact]
        public void StaticMethod()
        {
            TypeUtil<Dummy>.StaticMethod<Action>("BlahVoid").Invoke();

            Assert.Equal(42, TypeUtil<Dummy>.StaticMethod<Func<int>>("BlahFunc").Invoke());
        }

        [Fact]
        public void NonGenericPropertyViaProperties()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var properties = TypeUtil<Dummy>.Properties();
                var idProp = properties.Single(p => p.Property.Name == "ID");

                idProp.SetValue(dummy, 100);

                Assert.Equal(100, idProp.GetValue(dummy));
                Assert.Equal(100, dummy.ID);
            }
        }

        [Fact]
        public void NonGenericPropertyViaNonGenericProperties()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var properties = TypeUtil.Properties(typeof(Dummy));
                var idProp = properties.Single(p => p.Property.Name == "ID");

                idProp.SetValue(dummy, 100);

                Assert.Equal(100, idProp.GetValue(dummy));
                Assert.Equal(100, dummy.ID);
            }
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }

            public void InstanceVoid()
            {

            }

            public int InstanceFunc()
            {
                return 42;
            }

            public static void BlahVoid()
            {

            }

            private static int BlahFunc()
            {
                return 42;
            }
        }

        [Fact]
        public void AssignableFrom()
        {
            Assert.True(typeof(DateTime?).IsAssignableFrom(typeof(DateTime)));
            Assert.False(typeof(DateTime).IsAssignableFrom(typeof(DateTime?)));

            Assert.True(typeof(Base).IsAssignableFrom(typeof(Derived)));
            Assert.False(typeof(Derived).IsAssignableFrom(typeof(Base)));
        }

        private class Base
        {

        }

        private class Derived : Base
        {

        }

        [Fact]
        public void RepetitivePropDirect()
        {
            var dummy = new Dummy();

            for (var i = 0; i < 100000; i++)
            {
                dummy.ID = dummy.ID + 1;
            }
        }

        [Fact]
        public void RepetitivePropResolveTestWithLambda()
        {
            var dummy = new Dummy();
            System.Linq.Expressions.Expression<Func<Dummy, int>> expr = d => d.ID;

            for (var i = 0; i < 100000; i++)
            {
                var prop = TypeUtil<Dummy>.Property(expr);

                prop.SetValue(dummy, prop.GetValue(dummy) + 1);
            }
        }

        [Fact]
        public void RepetitivePropResolveTestWithName()
        {
            var dummy = new Dummy();

            for (var i = 0; i < 100000; i++)
            {
                var prop = TypeUtil<Dummy>.Property<int>("ID");

                prop.SetValue(dummy, prop.GetValue(dummy) + 1);
            }
        }

        [Fact]
        public void RepetitivePropResolveTestNonGeneric()
        {
            var dummy = new Dummy();

            for (var i = 0; i < 100000; i++)
            {
                var prop = TypeUtil.Property(typeof(Dummy), "ID");

                prop.SetValue(dummy, (int)prop.GetValue(dummy) + 1);
            }
        }

        [Fact]
        public void TypeNames()
        {
            Assert.Equal("Int32", TypeName.NameIncludingGenericArguments(typeof(int)));
            Assert.Equal("Nullable<Int32>", TypeName.NameIncludingGenericArguments(typeof(int?)));
            Assert.Equal("Dictionary<Int32, String>", TypeName.NameIncludingGenericArguments(typeof(Dictionary<int, string>)));
        }
    }
}
