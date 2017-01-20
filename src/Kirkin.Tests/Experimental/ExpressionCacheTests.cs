using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Kirkin.Linq.Expressions;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class ExpressionCacheTests
    {
        public ExpressionCacheTests()
        {
            // Warm up the JIT.
            var cache = new ExpressionCache<Dummy>();
            var expr = cache.GetOrAdd<int>(1, () => d => d.ID);
            var member = ExpressionUtil.Member(expr);
        }

        const int ITERATIONS = 1000000;

        [Test]
        public void RawBenchmark()
        {
            for (int i = 0; i < ITERATIONS; i++)
            {
                var id = ExpressionUtil.Member<Dummy>(d => d.ID);
            }
        }

        [Test]
        public void ExpressionCacheGetOrAddBenchmark()
        {
            var cache = new ExpressionCache<Dummy>();

            for (int i = 0; i < ITERATIONS; i++)
            {
                var id = ExpressionUtil.Member(cache.GetOrAdd<int>(1, () => d => d.ID));
            }
        }

        [Test]
        public void LocalCacheBenchmark()
        {
            Expression<Func<Dummy, int>> expr = d => d.ID;

            for (int i = 0; i < ITERATIONS; i++)
            {
                var id = ExpressionUtil.Member(expr);
            }
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        public class ExpressionCache<T>
        {
            // Access token is an Int32.
            private readonly Dictionary<int, Expression> Expressions = new Dictionary<int, Expression>();

            public ExpressionCache()
            {
            }

            public Expression<Func<T, TMember>> GetOrAdd<TMember>(int token, Func<Expression<Func<T, TMember>>> factory)
            {
                Expression<Func<T, TMember>> expr;

                if (this.TryGet(token, out expr))
                {
                    return expr;
                }

                expr = factory();

                this.Expressions.Add(token, expr);

                return expr;
            }

            public bool TryGet<TMember>(int token, out Expression<Func<T, TMember>> expr)
            {
                Expression exprObj;

                if (this.Expressions.TryGetValue(token, out exprObj))
                {
                    expr = (Expression<Func<T, TMember>>)exprObj;
                    return true;
                }

                expr = null;
                return false;
            }
        }
    }
}