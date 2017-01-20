using System;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Threading;

using NUnit.Framework;

namespace Kirkin.Tests.Threading
{
    public class AsyncSemaphoreTests
    {
        [Test]
        public void BasicApi()
        {
            AsyncSemaphore semaphore = new AsyncSemaphore(1, 1);

            Assert.AreEqual(1, semaphore.CurrentCount);
            Assert.AreEqual(TaskStatus.RanToCompletion, semaphore.WaitAsync().Status); // Synchronous completion.
            Assert.AreEqual(0, semaphore.CurrentCount);

            Task t2 = semaphore.WaitAsync();

            Assert.False(t2.IsCompleted);
            Assert.AreEqual(0, semaphore.CurrentCount);

            Task t3 = semaphore.WaitAsync();

            Assert.False(t3.IsCompleted);
            Assert.AreEqual(0, semaphore.CurrentCount);

            semaphore.Release();

            Assert.AreEqual(TaskStatus.RanToCompletion, t2.Status);
            Assert.False(t3.IsCompleted);
            Assert.Throws<InvalidOperationException>(() => semaphore.Release(releaseCount: 3));

            semaphore.Release();

            Assert.AreEqual(TaskStatus.RanToCompletion, t3.Status);
            Assert.AreEqual(0, semaphore.CurrentCount);

            semaphore.Release();

            Assert.AreEqual(1, semaphore.CurrentCount);
            Assert.Throws<InvalidOperationException>(() => semaphore.Release());
        }

        [Test]
        public void Cancellation()
        {
            AsyncSemaphore semaphore = new AsyncSemaphore(0, 1);
            CancellationTokenSource cts = new CancellationTokenSource();

            Task t1 = semaphore.WaitAsync(cts.Token);
            Task t2 = semaphore.WaitAsync();

            cts.Cancel();

            Assert.True(t1.IsCanceled);
            Assert.False(t2.IsCompleted);
            Assert.Throws<InvalidOperationException>(() => semaphore.Release(releaseCount: 3));

            semaphore.Release();

            Assert.AreEqual(TaskStatus.RanToCompletion, t2.Status);
        }
    }
}