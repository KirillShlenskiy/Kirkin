using System;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

using Xunit;

namespace Kirkin.Tests.Linq.Expressions
{
    public class ExpressionEngineTests
    {
        [Fact]
        public void FieldGetter()
        {
            Dummy dummy = new Dummy { ID = 123 };
            FieldInfo id = typeof(Dummy).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);
            Expression<Func<Dummy, int>> getter = ExpressionEngine.FieldGetter<Dummy, int>(id);

            Assert.Equal(123, getter.Compile().Invoke(dummy));
        }

        [Fact]
        public void FieldSetter()
        {
            Dummy dummy = new Dummy { ID = 0 };
            FieldInfo id = typeof(Dummy).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);
            Expression<Action<Dummy, int>> setter = ExpressionEngine.FieldSetter<Dummy, int>(id);

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
        }
    }
}