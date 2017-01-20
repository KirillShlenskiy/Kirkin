using System;
using System.Collections.Generic;
using System.Linq;

using Kirkin.Reflection;
using Kirkin.Utilities;

using NUnit.Framework;

namespace Kirkin.Tests.Reflection
{
    public class PropertyAccessorFactoryTests
    {
        [Test]
        public void PropertyResolutionBenchmark()
        {
            var id = typeof(Dummy).GetProperty("ID");

            for (int i = 0; i < 1000000; i++) {
                PropertyAccessorFactory.Resolve(id);
            }
        }

        [Test]
        public void PropertyByExpression()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = PropertyAccessorFactory<Dummy>.Resolve(d => d.ID);

                idProp.SetValue(dummy, 100);

                Assert.AreEqual(100, idProp.GetValue(dummy));
                Assert.AreEqual(100, dummy.ID);
            }
        }

        [Test]
        public void PropertyByName()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = PropertyAccessorFactory<Dummy>.Resolve<int>("ID");

                idProp.SetValue(dummy, 100);

                Assert.AreEqual(100, idProp.GetValue(dummy));
                Assert.AreEqual(100, dummy.ID);
            }
        }

        [Test]
        public void NonGenericPropertyByName()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = PropertyAccessorFactory.Resolve<Dummy>("ID");

                idProp.SetValue(dummy, 100);

                Assert.AreEqual(100, idProp.GetValue(dummy));
                Assert.AreEqual(100, dummy.ID);
            }
        }

        [Test]
        public void NonGenericPropertyByExpression()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = PropertyAccessorFactory.Resolve<Dummy>(d => d.ID);

                idProp.SetValue(dummy, 100);

                Assert.AreEqual(100, idProp.GetValue(dummy));
                Assert.AreEqual(100, dummy.ID);
            }
        }

        [Test]
        public void NonGenericPropertyAccessorFactoryNonGenericPropertyByName()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var idProp = PropertyAccessorFactory.Resolve(typeof(Dummy), "ID");

                idProp.SetValue(dummy, 100);

                Assert.AreEqual(100, idProp.GetValue(dummy));
                Assert.AreEqual(100, dummy.ID);
            }
        }

        [Test]
        public void NonGenericPropertyViaProperties()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var properties = PropertyAccessorFactory.ResolveAll<Dummy>();
                var idProp = properties.Single(p => p.Property.Name == "ID");

                idProp.SetValue(dummy, 100);

                Assert.AreEqual(100, idProp.GetValue(dummy));
                Assert.AreEqual(100, dummy.ID);
            }
        }

        [Test]
        public void NonGenericPropertyViaNonGenericProperties()
        {
            for (var i = 0; i < 100000; i++)
            {
                var dummy = new Dummy();
                var properties = PropertyAccessorFactory.ResolveAll(typeof(Dummy));
                var idProp = properties.Single(p => p.Property.Name == "ID");

                idProp.SetValue(dummy, 100);

                Assert.AreEqual(100, idProp.GetValue(dummy));
                Assert.AreEqual(100, dummy.ID);
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

        [Test]
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

        [Test]
        public void RepetitivePropDirect()
        {
            var dummy = new Dummy();

            for (var i = 0; i < 100000; i++)
            {
                dummy.ID = dummy.ID + 1;
            }
        }

        [Test]
        public void RepetitivePropResolveTestWithLambda()
        {
            var dummy = new Dummy();
            System.Linq.Expressions.Expression<Func<Dummy, int>> expr = d => d.ID;

            for (var i = 0; i < 100000; i++)
            {
                var prop = PropertyAccessorFactory<Dummy>.Resolve(expr);

                prop.SetValue(dummy, prop.GetValue(dummy) + 1);
            }
        }

        [Test]
        public void RepetitivePropResolveTestWithName()
        {
            var dummy = new Dummy();

            for (var i = 0; i < 100000; i++)
            {
                var prop = PropertyAccessorFactory<Dummy>.Resolve<int>("ID");

                prop.SetValue(dummy, prop.GetValue(dummy) + 1);
            }
        }

        [Test]
        public void RepetitivePropResolveTestNonGeneric()
        {
            var dummy = new Dummy();

            for (var i = 0; i < 100000; i++)
            {
                var prop = PropertyAccessorFactory.Resolve(typeof(Dummy), "ID");

                prop.SetValue(dummy, (int)prop.GetValue(dummy) + 1);
            }
        }

        [Test]
        public void TypeNames()
        {
            Assert.AreEqual("Int32", TypeName.NameIncludingGenericArguments(typeof(int)));
            Assert.AreEqual("Nullable<Int32>", TypeName.NameIncludingGenericArguments(typeof(int?)));
            Assert.AreEqual("Dictionary<Int32, String>", TypeName.NameIncludingGenericArguments(typeof(Dictionary<int, string>)));
        }
    }
}
