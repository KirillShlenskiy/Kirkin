using System;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Functional;

using Xunit;

namespace Kirkin.Tests.Functional
{
    public class ExecutableTests
    {
        [Fact]
        public void ExecuteWithRetrySucceeds()
        {
            var i = 0;

            Executable.Create(() => i++).Execute();
            Assert.Equal(1, i);
        }

        [Fact]
        public void ExecuteWithRetryViaFuncUtil()
        {
            var i = 0;
            Func<int> func = () => ++i;

            Assert.Equal(1, func.AsRetryable().OnException(3).Invoke());
        }

        [Fact]
        public async Task ExecuteWithRetryAsyncViaFuncUtil()
        {
            var i = 0;

            await Retryable
                .AsRetryable(async () =>
                {
                    await Task.Yield();
                    i++;
                })
                .OnException<InvalidOperationException>(2)
                .Invoke();
        }

        [Fact]
        public async Task ExecuteAsyncWithRetrySucceeds()
        {
            var i = 0;
            var ct = new CancellationToken();

            var executable = Executable.CreateAsync(async () =>
            {
                ct.ThrowIfCancellationRequested();

                await Task.Yield();

                i++;
            });

            await executable
                .RetryOnException<InvalidOperationException>(2)
                .ExecuteAsync();
        }

        [Fact]
        public void ExecuteWithRetrySucceedsOnLastRetry()
        {
            var i = 0;

            Executable
                .Create(() =>
                {
                    if (++i < 3) {
                        throw new InvalidOperationException();
                    }
                })
                .RetryOnException<InvalidOperationException>(2)
                .Execute();

            Assert.Equal(3, i);
        }

        [Fact]
        public async Task ExecuteAsyncWithRetrySucceedsOnLastRetry()
        {
            var i = 0;

            await Executable
                .CreateAsync(async () =>
                {
                    if (++i < 3)
                    {
                        await Task.Yield();

                        throw new InvalidOperationException();
                    }
                })
                .RetryOnException<InvalidOperationException>(2)
                .ExecuteAsync();

            Assert.Equal(3, i);
        }

        [Fact]
        public void ExecuteWithRetryFailsWhenRetryCountReached()
        {
            var i = 0;

            var buggyFunc = Executable.Create(() =>
            {
                i++;
                throw new InvalidOperationException();
            });

            var errored = false;

            try
            {
                buggyFunc.RetryOnException<InvalidOperationException>(2).Execute();
            }
            catch
            {
                errored = true;
            }

            Assert.True(errored);
            Assert.Equal(3, i);
        }

        [Fact]
        public async Task ExecuteAsyncWithRetryFailsWhenRetryCountReached()
        {
            var i = 0;

            var buggyFunc = Executable.CreateAsync(async () =>
            {
                await Task.Yield();
                i++;
                throw new InvalidOperationException();
            });

            var errored = false;

            try
            {
                await buggyFunc
                    .RetryOnException<InvalidOperationException>(2)
                    .ExecuteAsync();
            }
            catch
            {
                errored = true;
            }

            Assert.True(errored);
            Assert.Equal(3, i);
        }
    }
}
