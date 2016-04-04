using System;
using System.Linq.Expressions;

using Kirkin.Linq.Expressions;

using Xunit;

namespace Kirkin.Tests.Linq.Expressions
{
    public class ExpressionUtilTests
    {
        [Fact]
        public void MemberClass()
        {
            var dummy = new Dummy();
            var value = typeof(Dummy).GetProperty("Value");

            Assert.Equal(value, ExpressionUtil.Member<Dummy, string>(d => d.Value));
            Assert.Equal(value, ExpressionUtil.Member<string>(() => dummy.Value));
            Assert.Equal(value, ExpressionUtil.Member<Dummy>(d => d.Value));

            Expression<Func<object>> expr = () => new Dummy().Value;

            Assert.Equal(value, ExpressionUtil.Member(expr));
        }

        [Fact]
        public void MemberStruct()
        {
            var dummy = new Dummy();
            var id = typeof(Dummy).GetProperty("ID");

            Assert.Equal(id, ExpressionUtil.Member<Dummy, int>(d => d.ID));
            Assert.Equal(id, ExpressionUtil.Member<int>(() => dummy.ID));
            Assert.Equal(id, ExpressionUtil.Member<Dummy>(d => d.ID));

            Expression<Func<object>> expr = () => new Dummy().ID;

            Assert.Equal(id, ExpressionUtil.Member(expr));
        }

        [Fact]
        public void MemberNameClass()
        {
            var dummy = new Dummy();

            Assert.Equal("Value", ExpressionUtil.Member<Dummy, string>(d => d.Value).Name);
            Assert.Equal("Value", ExpressionUtil.Member<string>(() => dummy.Value).Name);
            Assert.Equal("Value", ExpressionUtil.Member<Dummy>(d => d.Value).Name);

            Expression<Func<object>> expr = () => new Dummy().Value;

            Assert.Equal("Value", ExpressionUtil.Member(expr).Name);
        }

        [Fact]
        public void MemberNameStruct()
        {
            var dummy = new Dummy();

            Assert.Equal("ID", ExpressionUtil.Member<Dummy, int>(d => d.ID).Name);
            Assert.Equal("ID", ExpressionUtil.Member<int>(() => dummy.ID).Name);
            Assert.Equal("ID", ExpressionUtil.Member<Dummy>(d => d.ID).Name);

            Expression<Func<object>> expr = () => new Dummy().ID;

            Assert.Equal("ID", ExpressionUtil.Member(expr).Name);
        }

        [Fact]
        public void PropertyClass()
        {
            var dummy = new Dummy();
            var value = typeof(Dummy).GetProperty("Value");

            Assert.Equal(value, ExpressionUtil.Property<Dummy, string>(d => d.Value));
            Assert.Equal(value, ExpressionUtil.Property<string>(() => dummy.Value));
            Assert.Equal(value, ExpressionUtil.Property<Dummy>(d => d.Value));

            Expression<Func<object>> expr = () => new Dummy().Value;

            Assert.Equal(value, ExpressionUtil.Property(expr));
        }

        [Fact]
        public void PropertyStruct()
        {
            var dummy = new Dummy();
            var id = typeof(Dummy).GetProperty("ID");

            Assert.Equal(id, ExpressionUtil.Property<Dummy, int>(d => d.ID));
            Assert.Equal(id, ExpressionUtil.Property<int>(() => dummy.ID));
            Assert.Equal(id, ExpressionUtil.Property<Dummy>(d => d.ID));

            Expression<Func<object>> expr = () => new Dummy().ID;

            Assert.Equal(id, ExpressionUtil.Property(expr));
        }

        [Fact]
        public void MixedExpressionTypes()
        {
            Expression<Func<Dummy, object>> idExpr = d => d.ID;
            Expression<Func<Dummy, object>> valueExpr = d => d.Value;

            // Check theory.
            Assert.NotNull(idExpr.Body as UnaryExpression);
            Assert.NotNull(valueExpr.Body as MemberExpression);

            // Check handling.
            Assert.NotNull(ExpressionUtil.Property(idExpr));
            Assert.NotNull(ExpressionUtil.Property(valueExpr));
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}