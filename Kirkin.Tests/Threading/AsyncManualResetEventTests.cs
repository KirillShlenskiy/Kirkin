using System.Threading.Tasks;

using Kirkin.Threading;

using Xunit;

namespace Kirkin.Tests.Threading
{
    public class AsyncManualResetEventTests
    {
        [Fact]
        public async Task Benchmark()
        {
            var mre = new AsyncManualResetEvent();

            for (int i = 0; i < 100000; i++)
            {
                var task = mre.WaitAsync();

                Assert.False(task.IsCompleted);

                mre.Set();

                // If WaitAsync is implemented properly, both
                // True and False tests will both fail intermittently.
                //Assert.False(task.IsCompleted);

                await task;

                mre.Reset();
            }
        }
    }
}