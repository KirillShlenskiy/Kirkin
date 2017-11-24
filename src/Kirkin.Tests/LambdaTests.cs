using System;
using System.Linq.Expressions;

using Kirkin.Linq.Expressions;

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

            Func<int> func = Lambda.ResolveAllCapturesViaCopy(
                // Will be rewritten as 1 + 3 + int.Parse("3") + 4.
                () => x + y + int.Parse(z) + container.Containee.Value
            );

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
                return ExpressionUtil
                    .ResolveAllFieldAndPropertyValuesAsConstants(expr)
                    .Compile();
            }
        }
    }
}