using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Caching;

using NUnit.Framework;

namespace Kirkin.Tests.Caching
{
    public class CacheParametrisationBenchmarks
    {
        public CacheParametrisationBenchmarks()
        {
            int value = 0;

            Cache.Create(value, i => i + 1);
            Cache.Create(() => value + 1);
        }

        const int Iterations = 1000000;

        [Test]
        public void ClosureBenchmarkFull()
        {
            int value = 0;
            ICache<int> cache = Cache.Create(() => value + 1);

            for (int i = 0; i < Iterations; i++)
            {
                Assert.False(cache.IsValid);
                Assert.AreEqual(1, cache.Value);
                cache.Invalidate();
            }
        }

        [Test]
        public void ParametrisedBenchmark()
        {
            int value = 0;
            ICache<int> cache = Cache.Create(value, i => i + 1);

            for (int i = 0; i < Iterations; i++)
            {
                Assert.False(cache.IsValid);
                Assert.AreEqual(1, cache.Value);
                cache.Invalidate();
            }
        }
    }

    public class CacheTests
    {
        [Test]
        public void Api()
        {
            ICache<int> cache;

            // Factory methods.
            cache = Cache.Constant(42);
            cache = Cache.Create(() => 1);
            cache = Cache.Create(1, i => i);
            cache = new AutoExpireCache<int>(() => 42, TimeSpan.FromSeconds(10));

            cache.Invalidate();
        }

        [Test]
        public void AutoExpireCacheBenchmarks()
        {
            AutoExpireCache<int> cache = new AutoExpireCache<int>(() => 42, Timeout.InfiniteTimeSpan);

            for (int i = 0; i < 100000; i++)
            {
                Assert.AreEqual(42, cache.Value);
                cache.Invalidate();
            }
        }

        [Test]
        public void LazyConcurrency()
        {
            for (int i = 0; i < 10; i++)
            {
                int valueFactoryCount = 0;

                ICache<string> cache = Cache.Create(() =>
                {
                    int cnt = Interlocked.Increment(ref valueFactoryCount);

                    Thread.Sleep(250);

                    return cnt.ToString();
                });

                string v = "0";

                Parallel.Invoke(
                    () => v = cache.Value,
                    () => v = cache.Value,
                    () => v = cache.Value,
                    () => v = cache.Value,
                    () => v = cache.Value
                );

                Assert.AreEqual(1, valueFactoryCount);
                Assert.AreEqual("1", v);

                cache.Invalidate();

                ConcurrentBag<string> values = new ConcurrentBag<string>();

                Task invalidateTask = Task.Run(() =>
                {
                    Thread.Sleep(100);
                    cache.Invalidate();
                });

                Parallel.Invoke(
                    () => values.Add(cache.Value),
                    () => values.Add(cache.Value),
                    () => values.Add(cache.Value),
                    () => values.Add(cache.Value)
                );

                invalidateTask.GetAwaiter().GetResult();

                Assert.AreEqual(4, values.Count);

                foreach (string value in values)
                {
                    Assert.AreEqual("3", value);
                }
            }
        }

        [Test]
        public void AutoExpireCache()
        {
            int obj = 0;

            ICache<string> cache = new AutoExpireCache<string>(
                () =>
                {
                    int val = Interlocked.Increment(ref obj);

                    Thread.Sleep(20);

                    return val.ToString();
                },
                TimeSpan.FromMilliseconds(50)
            );

            Assert.False(cache.IsValid);
            Assert.AreEqual("1", cache.Value);
            Assert.True(cache.IsValid);
            Assert.True(cache.IsValid);

            // Auto invalidation.
            Thread.Sleep(100);
            Assert.False(cache.IsValid);
            Assert.AreEqual("2", cache.Value);
            Assert.True(cache.IsValid);

            // Manual invalidation.
            cache.Invalidate();
            Assert.False(cache.IsValid);

            // Invalidation in flight.
            Task<string> skipOneTask = Task.Run(() => cache.Value);

            Thread.Sleep(10);
            cache.Invalidate();

            Assert.AreEqual("4", skipOneTask.Result);
        }

        [Test]
        public void AutoExpiryCacheNoExpiry()
        {
            ICache<int> cache = new AutoExpireCache<int>(() => 0, Timeout.InfiniteTimeSpan);

            //Assert.AreEqual(Timeout.InfiniteTimeSpan, cache.ExpireAfter);

            ICache<int> cacheShim = cache;

            Assert.False(cacheShim.IsValid);
            Assert.AreEqual(0, cache.Value);
            Assert.True(cacheShim.IsValid);
        }
    }
}