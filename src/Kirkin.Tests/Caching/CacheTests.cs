using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Caching;

using Xunit;

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

        [Fact]
        public void ClosureBenchmarkFull()
        {
            int value = 0;
            ICache<int> cache = Cache.Create(() => value + 1);

            for (int i = 0; i < Iterations; i++)
            {
                Assert.False(cache.IsValid);
                Assert.Equal(1, cache.Value);
                cache.Invalidate();
            }
        }

        //[Fact]
        //public void ClosureBenchmarkPublicationOnly()
        //{
        //    int value = 0;
        //    ICache<int> cache = Cache.Create(() => value + 1, CacheThreadSafetyMode.PublicationOnly);

        //    for (int i = 0; i < Iterations; i++)
        //    {
        //        Assert.False(cache.IsValid);
        //        Assert.Equal(1, cache.Value);
        //        cache.Invalidate();
        //    }
        //}

        [Fact]
        public void ParametrisedBenchmark()
        {
            int value = 0;
            ICache<int> cache = Cache.Create(value, i => i + 1);

            for (int i = 0; i < Iterations; i++)
            {
                Assert.False(cache.IsValid);
                Assert.Equal(1, cache.Value);
                cache.Invalidate();
            }
        }
    }

    public class CacheTests
    {
        [Fact]
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

        [Fact]
        public void AutoExpireCacheBenchmarks()
        {
            var cache = new AutoExpireCache<int>(() => 42, Timeout.InfiniteTimeSpan);

            for (int i = 0; i < 100000; i++)
            {
                Assert.Equal(42, cache.Value);
                cache.Invalidate();
            }
        }

        [Fact]
        public void LazyConcurrency()
        {
            for (var i = 0; i < 10; i++)
            {
                var valueFactoryCount = 0;

                var cache = Cache.Create(() =>
                {
                    var cnt = Interlocked.Increment(ref valueFactoryCount);

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

                Assert.Equal(1, valueFactoryCount);
                Assert.Equal("1", v);

                cache.Invalidate();

                var values = new ConcurrentBag<string>();

                var invalidateThread = new Thread(() =>
                {
                    Thread.Sleep(100);
                    cache.Invalidate();
                });

                invalidateThread.Start();

                Parallel.Invoke(
                    () => values.Add(cache.Value),
                    () => values.Add(cache.Value),
                    () => values.Add(cache.Value),
                    () => values.Add(cache.Value)
                );

                invalidateThread.Join();

                Assert.Equal(4, values.Count);

                foreach (var value in values)
                {
                    Assert.Equal("3", value);
                }
            }
        }

        //[Fact]
        //public void InterlockedConcurrency()
        //{
        //    for (var i = 0; i < 10; i++)
        //    {
        //        var valueFactoryCount = 0;

        //        var cache = Cache.Create(
        //            () =>
        //            {
        //                var cnt = Interlocked.Increment(ref valueFactoryCount);

        //                Thread.Sleep(250);

        //                return cnt.ToString();
        //            },
        //            CacheThreadSafetyMode.PublicationOnly
        //        );

        //        string v = "0";

        //        Parallel.Invoke(
        //            () => v = cache.Value,
        //            () => v = cache.Value,
        //            () => v = cache.Value,
        //            () => v = cache.Value,
        //            () => v = cache.Value
        //        );

        //        Assert.Equal(5, valueFactoryCount);
        //        //Assert.AreEqual("5", v); - this number will be completely random.

        //        cache.Invalidate();

        //        var values = new ConcurrentBag<string>();

        //        var invalidateThread = new Thread(() =>
        //        {
        //            Thread.Sleep(100);
        //            cache.Invalidate();
        //        });

        //        invalidateThread.Start();

        //        Parallel.Invoke(
        //            () => values.Add(cache.Value),
        //            () => values.Add(cache.Value),
        //            () => values.Add(cache.Value),
        //            () => values.Add(cache.Value)
        //        );

        //        invalidateThread.Join();

        //        Assert.Equal(4, values.Count);

        //        //foreach (var value in values)
        //        //{
        //        //    Assert.AreEqual("3", value);
        //        //}
        //    }
        //}

        [Fact]
        public void AutoExpireCache()
        {
            int obj = 0;

            ICache<string> cache = new AutoExpireCache<string>(
                () =>
                {
                    var val = Interlocked.Increment(ref obj);

                    Thread.Sleep(20);

                    return val.ToString();
                },
                TimeSpan.FromMilliseconds(50)
            );

            Assert.False(cache.IsValid);
            Assert.Equal("1", cache.Value);
            Assert.True(cache.IsValid);
            Assert.True(cache.IsValid);

            // Auto invalidation.
            Thread.Sleep(100);
            Assert.False(cache.IsValid);
            Assert.Equal("2", cache.Value);
            Assert.True(cache.IsValid);

            // Manual invalidation.
            cache.Invalidate();
            Assert.False(cache.IsValid);

            // Invalidation in flight.
            var skipOneTask = Task.Run(() => cache.Value);

            Thread.Sleep(10);
            cache.Invalidate();

            Assert.Equal("4", skipOneTask.Result);
        }

        [Fact]
        public void AutoExpiryCacheNoExpiry()
        {
            ICache<int> cache = new AutoExpireCache<int>(() => 0, Timeout.InfiniteTimeSpan);

            //Assert.Equal(Timeout.InfiniteTimeSpan, cache.ExpireAfter);

            ICache<int> cacheShim = cache;

            Assert.False(cacheShim.IsValid);
            Assert.Equal(0, cache.Value);
            Assert.True(cacheShim.IsValid);
        }
    }
}