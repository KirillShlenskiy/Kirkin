using System;
using System.Linq.Expressions;
using System.Reflection;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class LambdaTests
    {
        [Test]
        public void RegularClosureTest()
        {
            int x = 1;
            int y = 2;
            string z = "3";

            Func<int> func = () => x + y + int.Parse(z);

            Assert.AreEqual(6, func());

            x = 0;
            y = 0;
            z = "0";

            Assert.AreEqual(0, func());
        }

        [Test]
        public void CopyClosureTest()
        {
            int x = 1;
            int y = 2;
            string z = "3";

            Func<int> func = Lambda.CopyAllCaptures(() => x + y + int.Parse(z));

            Assert.AreEqual(6, func());

            x = 0;
            y = 0;
            z = "0";

            Assert.AreEqual(6, func());
        }

        static class Lambda
        {
            public static Func<T> CopyAllCaptures<T>(Expression<Func<T>> expr)
            {
                Expression newBody = ConstantResolutionVisitor.Instance.Visit(expr.Body);

                if (newBody == expr.Body) {
                    return expr.Compile(); // Unmodified.
                }

                return Expression
                    .Lambda<Func<T>>(newBody)
                    .Compile();
            }

            sealed class ConstantResolutionVisitor : ExpressionVisitor
            {
                internal static readonly ConstantResolutionVisitor Instance = new ConstantResolutionVisitor();

                protected override Expression VisitMember(MemberExpression node)
                {
                    ConstantExpression constExpr = (ConstantExpression)node.Expression;
                    object obj = constExpr.Value;

                    if (node.Member is PropertyInfo prop) {
                        return Expression.Constant(prop.GetValue(obj));
                    }

                    if (node.Member is FieldInfo field) {
                        return Expression.Constant(field.GetValue(obj));
                    }

                    return base.VisitMember(node);
                }
            }
        }
    }
}