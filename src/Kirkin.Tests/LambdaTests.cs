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

            DodgyContainer container = new DodgyContainer {
                Containee = new DodgyContainee {
                    Value = 4
                }
            };

            Func<int> func = () => x + y + int.Parse(z) + container.Containee.Value;

            Assert.AreEqual(10, func());

            x = 0;
            y = 0;
            z = "0";

            Assert.AreEqual(4, func());

            container.Containee.Value = 0;

            Assert.AreEqual(0, func());

            container.Containee = null;

            Assert.Throws<NullReferenceException>(() => func());
        }

        [Test]
        public void CopyClosureTest()
        {
            int x = 1;
            int y = 2;
            string z = "3";

            DodgyContainer container = new DodgyContainer {
                Containee = new DodgyContainee {
                    Value = 4
                }
            };

            Func<int> func = Lambda.ResolveAllCapturesViaCopy(() => x + y + int.Parse(z) + container.Containee.Value);

            Assert.AreEqual(10, func());

            x = 0;
            y = 0;
            z = "0";

            Assert.AreEqual(10, func());

            container.Containee.Value = 0;

            Assert.AreEqual(10, func());

            container.Containee = null;

            Assert.AreEqual(10, func());
        }

        class DodgyContainer
        {
            public DodgyContainee Containee;
        }

        class DodgyContainee
        {
            public int Value;
        }

        static class Lambda
        {
            public static Func<T> ResolveAllCapturesViaCopy<T>(Expression<Func<T>> expr)
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
                    ConstantExpression constExpr = node.Expression as ConstantExpression;

                    if (node.Expression is MemberExpression memberExpr)
                    {
                        Expression memberValueExpr = VisitMember(memberExpr); // Reduce.

                        if (memberValueExpr is ConstantExpression newConstExpr)
                        {
                            constExpr = newConstExpr;
                        }
                        else
                        {
                            return memberValueExpr;
                        }
                    }

                    if (constExpr != null)
                    {
                        object obj = constExpr.Value;

                        if (node.Member is PropertyInfo prop) {
                            return Expression.Constant(prop.GetValue(obj));
                        }

                        if (node.Member is FieldInfo field) {
                            return Expression.Constant(field.GetValue(obj));
                        }
                    }

                    return base.VisitMember(node);
                }
            }
        }
    }
}