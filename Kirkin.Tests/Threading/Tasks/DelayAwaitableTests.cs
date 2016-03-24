using System;
using System.Threading.Tasks;

using Kirkin.Threading.Tasks;

using Xunit;

namespace Kirkin.Tests.Threading.Tasks
{
    public class DelayAwaitableTests
    {
        [Fact]
        public async Task CompletionSource()
        {
            for (int i = 0; i < 100; i++)
            {
                var source = new AwaitableCompletionSource<int>();
                int result = 0;

                Task t1 = Task.Run(async () => result = await source);

                await new DelayAwaitable(10);

                Assert.False(t1.IsCompleted);

                source.SetResult(42); // Complete asynchronously.

                Assert.False(t1.IsCompleted);

                await new DelayAwaitable(10);

                Assert.True(t1.IsCompleted);
                Assert.Equal(42, result);
            }
        }

        [Fact]
        public async Task BasicNoDelay()
        {
            for (int i = 0; i < 10; i++)
            {
                await new DelayAwaitable();
            }
        }

        [Fact]
        public async Task BasicDelay()
        {
            for (int i = 0; i < 10; i++)
            {
                await new DelayAwaitable(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}