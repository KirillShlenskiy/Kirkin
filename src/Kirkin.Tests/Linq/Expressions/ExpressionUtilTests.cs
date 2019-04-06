using System;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

using NUnit.Framework;

namespace Kirkin.Tests.Linq.Expressions
{
    public class ExpressionUtilTests
    {
        [Test]
        public void MemberClass()
        {
            var dummy = new Dummy();
            var value = typeof(Dummy).GetProperty("Value");

            Assert.AreEqual(value, ExpressionUtil.Member<Dummy, string>(d => d.Value));
            Assert.AreEqual(value, ExpressionUtil.Member(() => dummy.Value));
            Assert.AreEqual(value, ExpressionUtil.Member<Dummy>(d => d.Value));

            Expression<Func<object>> expr = () => new Dummy().Value;

            Assert.AreEqual(value, ExpressionUtil.Member(expr));
        }

        [Test]
        public void MemberStruct()
        {
            var dummy = new Dummy();
            var id = typeof(Dummy).GetProperty("ID");

            Assert.AreEqual(id, ExpressionUtil.Member<Dummy, int>(d => d.ID));
            Assert.AreEqual(id, ExpressionUtil.Member(() => dummy.ID));
            Assert.AreEqual(id, ExpressionUtil.Member<Dummy>(d => d.ID));

            Expression<Func<object>> expr = () => new Dummy().ID;

            Assert.AreEqual(id, ExpressionUtil.Member(expr));
        }

        [Test]
        public void MemberNameClass()
        {
            var dummy = new Dummy();

            Assert.AreEqual("Value", ExpressionUtil.Member<Dummy, string>(d => d.Value).Name);
            Assert.AreEqual("Value", ExpressionUtil.Member(() => dummy.Value).Name);
            Assert.AreEqual("Value", ExpressionUtil.Member<Dummy>(d => d.Value).Name);

            Expression<Func<object>> expr = () => new Dummy().Value;

            Assert.AreEqual("Value", ExpressionUtil.Member(expr).Name);
        }

        [Test]
        public void MemberNameStruct()
        {
            var dummy = new Dummy();

            Assert.AreEqual("ID", ExpressionUtil.Member<Dummy, int>(d => d.ID).Name);
            Assert.AreEqual("ID", ExpressionUtil.Member(() => dummy.ID).Name);
            Assert.AreEqual("ID", ExpressionUtil.Member<Dummy>(d => d.ID).Name);

            Expression<Func<object>> expr = () => new Dummy().ID;

            Assert.AreEqual("ID", ExpressionUtil.Member(expr).Name);
        }

        [Test]
        public void PropertyClass()
        {
            var dummy = new Dummy();
            var value = typeof(Dummy).GetProperty("Value");

            Assert.AreEqual(value, ExpressionUtil.Property<Dummy, string>(d => d.Value));
            Assert.AreEqual(value, ExpressionUtil.Property(() => dummy.Value));
            Assert.AreEqual(value, ExpressionUtil.Property<Dummy>(d => d.Value));

            Expression<Func<object>> expr = () => new Dummy().Value;

            Assert.AreEqual(value, ExpressionUtil.Property(expr));
        }

        [Test]
        public void PropertyStruct()
        {
            var dummy = new Dummy();
            var id = typeof(Dummy).GetProperty("ID");

            Assert.AreEqual(id, ExpressionUtil.Property<Dummy, int>(d => d.ID));
            Assert.AreEqual(id, ExpressionUtil.Property(() => dummy.ID));
            Assert.AreEqual(id, ExpressionUtil.Property<Dummy>(d => d.ID));

            Expression<Func<object>> expr = () => new Dummy().ID;

            Assert.AreEqual(id, ExpressionUtil.Property(expr));
        }

        [Test]
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

        [Test]
        public void MethodExpressions()
        {
            MethodInfo @void = typeof(Dummy).GetMethod("Void");
            MethodInfo get42 = typeof(Dummy).GetMethod("Get", new Type[0]);
            MethodInfo get = typeof(Dummy).GetMethod("Get", new[] { typeof(string) });

            Assert.NotNull(@void);
            Assert.NotNull(get42);
            Assert.NotNull(get);

            Assert.AreEqual(@void, ExpressionUtil.InstanceMethod<Dummy>(d => d.Void()));
            Assert.AreEqual(get42, ExpressionUtil.InstanceMethod<Dummy>(d => d.Get()));
            Assert.AreEqual(get, ExpressionUtil.InstanceMethod<Dummy>(d => d.Get("zzz")));

            MethodInfo consoleWriteLine = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });

            Assert.AreEqual(consoleWriteLine, ExpressionUtil.StaticMethod(() => Console.WriteLine((string)null)));
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }

            public void Void() { }
            public int Get() => 42;
            public string Get(string s) => s;
        }
    }
}