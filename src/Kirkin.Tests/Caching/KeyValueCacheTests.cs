using System;
using System.Threading.Tasks;

using Kirkin.Caching;
using Kirkin.Caching.Internal;

using NUnit.Framework;

namespace Kirkin.Tests.Caching
{
    public class KeyValueCacheTests
    {
        [Test]
        public async Task ShortLivedVolatileCache()
        {
            int count = 0;
            ICache<int> cache = null;

            Action scheduleInvalidate = async () =>
            {
                // Can't use yield because we don't have a SynchronizationContext.
                await Task.Delay(100);

                cache.Invalidate();
            };

            cache = Cache.Create(
                () => 42,
                key =>
                {
                    count++;

                    scheduleInvalidate();

                    return key + 1;
                }
            );

            Assert.False(cache.IsValid);
            Assert.AreEqual(43, cache.Value);
            Assert.True(cache.IsValid);
            Assert.AreEqual(1, count);
            Assert.AreEqual(43, cache.Value);
            Assert.True(cache.IsValid);
            Assert.AreEqual(1, count);

            await Task.Delay(200);

            Assert.False(cache.IsValid);
            Assert.AreEqual(1, count);
            Assert.AreEqual(43, cache.Value);
            Assert.AreEqual(2, count);

            //cache.Invalidate(41); // Miss.

            //Assert.True(cache.IsValid(42));
            //Assert.AreEqual(43, cache.GetValue(42));
            //Assert.AreEqual(2, count);

            cache.Invalidate(); // Hit.

            Assert.False(cache.IsValid);
            Assert.AreEqual(43, cache.Value);
            Assert.True(cache.IsValid);
            Assert.AreEqual(3, count);
        }

        [Test]
        public async Task ShortLivedCache()
        {
            int count = 0;

            IKeyValueCache<int, int> cache = null;

            // Short-lived cache simulation.
            // Can't use yield because we don't have a SynchronizationContext.
            Action<int> scheduleInvalidate = async key =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                cache.Invalidate(key);
            };

            cache = new SingleKeyValueCache<int, int>(key =>
            {
                count++;

                scheduleInvalidate(key);

                return key + 1;
            });

            Assert.False(cache.IsValid(42));
            Assert.AreEqual(43, cache.GetValue(42));
            Assert.True(cache.IsValid(42));
            Assert.AreEqual(1, count);
            Assert.AreEqual(43, cache.GetValue(42));
            Assert.True(cache.IsValid(42));
            Assert.AreEqual(1, count);

            await Task.Delay(200);

            Assert.False(cache.IsValid(42));
            Assert.AreEqual(1, count);
            Assert.AreEqual(43, cache.GetValue(42));
            Assert.AreEqual(2, count);

            cache.Invalidate(41); // Miss.

            Assert.True(cache.IsValid(42));
            Assert.AreEqual(43, cache.GetValue(42));
            Assert.AreEqual(2, count);

            cache.Invalidate(42); // Hit.

            Assert.False(cache.IsValid(42));
            Assert.AreEqual(43, cache.GetValue(42));
            Assert.True(cache.IsValid(42));
            Assert.AreEqual(3, count);
        }
    }
}