using System;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

using Xunit;

namespace Kirkin.Tests.Linq.Expressions
{
    public class ExpressionEngineTests
    {
        [Fact] // 155
        public void Perf()
        {
            FieldInfo id = typeof(Dummy).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);

            for (int i = 0; i < 100000; i++) {
                ExpressionEngine.Getter<Dummy, int>(id);
            }
        }

        [Fact]
        public void FieldGetter()
        {
            Dummy dummy = new Dummy { ID = 123 };
            FieldInfo id = typeof(Dummy).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);
            Expression<Func<Dummy, int>> getter = ExpressionEngine.Getter<Dummy, int>(id);

            Assert.Equal(123, getter.Compile().Invoke(dummy));
        }

        [Fact]
        public void FieldSetter()
        {
            Dummy dummy = new Dummy { ID = 0 };
            FieldInfo id = typeof(Dummy).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);
            Expression<Action<Dummy, int>> setter = ExpressionEngine.Setter<Dummy, int>(id);

            setter.Compile().Invoke(dummy, 123);

            Assert.Equal(123, dummy.ID);
        }

        [Fact]
        public void PropertyGetter()
        {
            Dummy dummy = new Dummy { ID = 123 };
            PropertyInfo id = typeof(Dummy).GetProperty("ID");
            Expression<Func<Dummy, int>> getter = ExpressionEngine.Getter<Dummy, int>(id);

            Assert.Equal(123, getter.Compile().Invoke(dummy));
        }

        [Fact]
        public void PropertySetter()
        {
            Dummy dummy = new Dummy { ID = 0 };
            PropertyInfo id = typeof(Dummy).GetProperty("ID");
            Expression<Action<Dummy, int>> setter = ExpressionEngine.Setter<Dummy, int>(id);

            setter.Compile().Invoke(dummy, 123);

            Assert.Equal(123, dummy.ID);
        }

        sealed class Dummy
        {
            private int _id;

            public int ID
            {
                get
                {
                    return _id;
                }
                set
                {
                    _id = value;
                }
            }

            private int AddOne(int input)
            {
                return input + 1;
            }

            public override string ToString()
            {
                return ID.ToString();
            }
        }

        [Fact]
        public void InstanceMethodParameterless()
        {
            Dummy dummy = new Dummy { ID = 123 };

            Func<Dummy, string> func = ExpressionEngine
                .Method<Dummy>()
                .Func<string>("ToString")
                .Compile();

            Assert.Equal("123", func(dummy));
        }

        [Fact]
        public void InstanceMethodParameterlessCaseInsensitive()
        {
            Dummy dummy = new Dummy { ID = 123 };

            Func<Dummy, string> func = ExpressionEngine
                .Method<Dummy>()
                .Func<string>("tostring", ignoreCase: true)
                .Compile();

            Assert.Equal("123", func(dummy));
        }

        [Fact]
        public void InstanceMethodWithOneParameter()
        {
            Dummy dummy = new Dummy { ID = 123 };

            Func<Dummy, int, int> func = ExpressionEngine
                .Method<Dummy>()
                .Func<int, int>("AddOne", nonPublic: true)
                .Compile();

            Assert.Equal(43, func(dummy, 42));
        }
    }
}