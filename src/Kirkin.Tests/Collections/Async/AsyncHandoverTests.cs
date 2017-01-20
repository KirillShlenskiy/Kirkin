using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Kirkin.Collections.Async;

namespace Kirkin.Tests.Collections.Async
{
    public class AsyncHandoverTests
    {
        [Test]
        public void AddThrowsAfterCompleteAdding()
        {
            var handover = new AsyncHandover<int>();

            handover.CompleteAdding();
            Assert.Throws<InvalidOperationException>(() => handover.Add(1));
        }

        public async Task Simple()
        {
            var handover = new AsyncHandover<int>();

            Assert.AreEqual(0, handover.Count);

            handover.Add(1);

            Assert.AreEqual(1, handover.Count);

            handover.Add(2);

            Assert.AreEqual(2, handover.Count);
            Assert.AreEqual(0, handover.WaiterCount);

            TakeResult<int> result;

            Assert.True((result = await handover.TryTakeAsync()).Success);
            Assert.AreEqual(1, result.Value);
            Assert.True((result = await handover.TryTakeAsync()).Success);
            Assert.AreEqual(2, result.Value);
            Assert.AreEqual(0, handover.Count);

            var next = handover.TryTakeAsync();

            Assert.AreEqual(1, handover.WaiterCount);

            // Ensure that we haven't completed synchronously.
            Assert.False(next.IsCompleted);

            handover.Add(3);

            Assert.AreEqual(0, handover.Count);
            Assert.AreEqual(0, handover.WaiterCount); // Risky one.
            Assert.True((result = await next).Success);
            Assert.AreEqual(3, result.Value);

            handover.CompleteAdding();

            Assert.False((await handover.TryTakeAsync()).Success);
        }

        public async Task Pipeline()
        {
            var handover = new AsyncHandover<int>();

            var consumer = Task.Run(async () =>
            {
                var i = 0;
                TakeResult<int> result;

                while ((result = await handover.TryTakeAsync()).Success)
                {
                    await Task.Delay(60);

                    Assert.AreEqual(i, result.Value);

                    i++;
                }
            });

            await Task.Delay(100);

            var producer = Task.Run(async () =>
            {
                try
                {
                    for (var i = 0; i < 10; i++)
                    {
                        await Task.Delay(50);

                        handover.Add(i);
                    }
                }
                finally
                {
                    handover.CompleteAdding();
                }
            });

            await Task.WhenAll(producer, consumer);
        }

        [Test]
        public async Task ThreadSafety()
        {
            var queue = new BoundedCapacityAsyncHandover<int>(1);

            var producer1 = Task.Run(async () =>
            {
                Debug.Print("Producer 1 started.");

                for (var i = 0; i < 10000; i++)
                {
                    await queue.AddAsync(i);
                }
            });

            var producer2 = Task.Run(async () =>
            {
                Debug.Print("Producer 2 started.");

                for (var i = 10000; i < 20000; i++)
                {
                    await queue.AddAsync(i);
                }
            });

            var result = new List<int>();

            var consumer1 = Task.Run(async () =>
            {
                Debug.Print("Consumer 1 started.");

                for (;;)
                {
                    var takeResult = await queue.TryTakeAsync();

                    if (!takeResult.Success) return;

                    lock (result)
                    {
                        result.Add(takeResult.Value);
                    }
                }
            });

            var consumer2 = Task.Run(async () =>
            {
                for (;;)
                {
                    Debug.Print("Consumer 2 started.");

                    var takeResult = await queue.TryTakeAsync();

                    if (!takeResult.Success) return;

                    lock (result)
                    {
                        result.Add(takeResult.Value);
                    }
                }
            });

            await Task.WhenAll(producer1, producer2);

            queue.CompleteAdding();

            await Task.WhenAll(consumer1, consumer2);

            Assert.AreEqual(20000, result.Count);
            Assert.True(Enumerable.Range(0, 20000).SequenceEqual(result.OrderBy(i => i)));
        }

        [Test]
        public async Task Cancellation()
        {
            var queue = new AsyncHandover<int>();

            var producer = Task.Run(async () =>
            {
                queue.Add(1);

                await Task.Delay(TimeSpan.FromMilliseconds(250));

                queue.Add(2);
                queue.CompleteAdding();
            });

            var consumer = Task.Run(async () =>
            {
                Assert.AreEqual(1, (await queue.TryTakeAsync().ConfigureAwait(false)).Value);

                var prematureCancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                var canceled = false;

                try
                {
                    await queue.TryTakeAsync(prematureCancellation.Token).ConfigureAwait(false);

                    Debug.Assert(true); // Breakpoint.
                }
                catch (OperationCanceledException)
                {
                    // Expected.
                    canceled = true;
                }

                if (!canceled)
                {
                    throw new InvalidOperationException();
                }

                Assert.AreEqual(2, (await queue.TryTakeAsync().ConfigureAwait(false)).Value);
            });

            await Task.WhenAll(producer, consumer);
        }

        [Test]
        public async Task BoundedCapacity()
        {
            var handover = new BoundedCapacityAsyncHandover<int>(2);

            var producer = Task.Run(async () =>
            {
                try
                {
                    for (var i = 0; i < 100; i++)
                    {
                        Assert.True(handover.Count <= handover.BoundedCapacity);

                        var task = handover.AddAsync(i);

                        await task.ConfigureAwait(false);
                    }
                }
                finally
                {
                    handover.CompleteAdding();
                }
            });

            var consumer = Task.Run(async () =>
            {
                while (true)
                {
                    var result = await handover.TryTakeAsync().ConfigureAwait(false);

                    if (!result.Success) return;

                    await Task.Delay(10).ConfigureAwait(false);
                }
            });

            await Task.WhenAll(producer, consumer);
        }
    }
}