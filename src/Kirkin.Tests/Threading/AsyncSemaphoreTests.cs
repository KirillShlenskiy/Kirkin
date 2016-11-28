using System;
using System.Threading.Tasks;

using Kirkin.Threading;

using Xunit;

namespace Kirkin.Tests.Threading
{
    public class AsyncSemaphoreTests
    {
        [Fact]
        public void BasicApi()
        {
            AsyncSemaphore semaphore = new AsyncSemaphore(1, 1);

            Assert.Equal(1, semaphore.CurrentCount);
            Assert.Equal(TaskStatus.RanToCompletion, semaphore.WaitAsync().Status); // Synchronous completion.
            Assert.Equal(0, semaphore.CurrentCount);

            Task t2 = semaphore.WaitAsync();

            Assert.False(t2.IsCompleted);
            Assert.Equal(0, semaphore.CurrentCount);

            Task t3 = semaphore.WaitAsync();

            Assert.False(t3.IsCompleted);
            Assert.Equal(0, semaphore.CurrentCount);

            semaphore.Release();

            Assert.Equal(TaskStatus.RanToCompletion, t2.Status);
            Assert.False(t3.IsCompleted);
            Assert.Throws<InvalidOperationException>(() => semaphore.Release(releaseCount: 3));

            semaphore.Release();

            Assert.Equal(TaskStatus.RanToCompletion, t3.Status);
            Assert.Equal(0, semaphore.CurrentCount);

            semaphore.Release();

            Assert.Equal(1, semaphore.CurrentCount);
            Assert.Throws<InvalidOperationException>(() => semaphore.Release());
        }
    }
}