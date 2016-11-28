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

            Assert.Equal(TaskStatus.RanToCompletion, semaphore.WaitAsync().Status); // Synchronous completion.

            Task t2 = semaphore.WaitAsync();

            Assert.False(t2.IsCompleted);

            Task t3 = semaphore.WaitAsync();

            Assert.False(t3.IsCompleted);

            semaphore.Release();

            Assert.Equal(TaskStatus.RanToCompletion, t2.Status);
            Assert.False(t3.IsCompleted);

            semaphore.Release();

            Assert.Equal(TaskStatus.RanToCompletion, t3.Status);
        }
    }
}