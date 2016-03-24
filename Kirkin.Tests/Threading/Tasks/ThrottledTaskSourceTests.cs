using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Threading.Tasks;

using Xunit;

namespace Kirkin.Tests.Threading.Tasks
{
    public class ThrottledTaskSourceTests
    {
        //[Fact]
        public async Task MemDiagnostics()
        {
            var diffs = new List<long>();

            for (int i = 0; i < 10; i++)
            {
                var memory1 = GC.GetTotalMemory(true);

                await SelfCancellation();

                var memory2 = GC.GetTotalMemory(false);
                var diff = memory2 - memory1;

                diffs.Add(diff);

                GC.Collect();
                GC.Collect();
            }

            // Inspect diff.
            Debug.Print("Done.");
        }

        [Fact]
        public async Task SelfCancellation()
        {
            DelayTaskSource source = null;
            int count = 0;
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                var newSource = new DelayTaskSource(TimeSpan.FromMilliseconds(100));
                var oldSource = Interlocked.Exchange(ref source, newSource);

                if (oldSource != null) {
                    oldSource.Dispose();
                }

                var task = newSource.DelayTask.ContinueWith(_ => Interlocked.Increment(ref count), TaskContinuationOptions.OnlyOnRanToCompletion);
                
                tasks.Add(task);

                await Task.Delay(10);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Expected.
            }

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task SelfCancellation2()
        {
            var source = new DelayTaskFactory();
            int count = 0;
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                var task = source.Restart(TimeSpan.FromMilliseconds(50)).ContinueWith(_ => Interlocked.Increment(ref count), TaskContinuationOptions.OnlyOnRanToCompletion);
                
                tasks.Add(task);

                await Task.Delay(10);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Expected.
            }

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task ParallelSelfCancellation()
        {
            DelayTaskSource source = null;
            int count = 0;
            var tasks = new List<Task>();

            Parallel.For(0, 1000, i =>
            {
                var newSource = new DelayTaskSource(TimeSpan.FromMilliseconds(50));
                var oldSource = Interlocked.Exchange(ref source, newSource);

                if (oldSource != null)
                {
                    oldSource.Dispose();
                }

                var task = newSource.DelayTask.ContinueWith(_ => Interlocked.Increment(ref count), TaskContinuationOptions.OnlyOnRanToCompletion);

                lock(tasks) {
                    tasks.Add(task);
                }

                Thread.Sleep(5);
            });

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Expected.
            }

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task ParallelSelfCancellation2()
        {
            var source = new DelayTaskFactory(DelayTaskCancellationMode.SetTaskResultToFalse);
            int count = 0;
            var tasks = new List<Task>();

            Parallel.For(0, 1000, i =>
            {
                var task = source
                    .Restart(TimeSpan.FromMilliseconds(50))
                    .ContinueWith(t =>
                    {
                        if (t.Result) {
                            Interlocked.Increment(ref count);
                        }
                    });

                lock(tasks) {
                    tasks.Add(task);
                }

                Thread.Sleep(5);
            });

            await Task.WhenAll(tasks);

            Assert.Equal(1, count);
        }

        [Fact]
        public void StressTesting()
        {
            Task lastTask = null;
            DelayTaskSource source = null;

            for (int i = 0; i < 1000000; i++)
            {
                var newSource = new DelayTaskSource(TimeSpan.FromMilliseconds(10));
                var oldSource = Interlocked.Exchange(ref source, newSource);

                if (oldSource != null) {
                    oldSource.Dispose();
                }

                lastTask = source.DelayTask;
            }

            lastTask.Wait();
        }

        [Fact]
        public void StressTesting2()
        {
            var source = new DelayTaskFactory();
            Task lastTask = null;

            for (int i = 0; i < 1000000; i++)
            {
                lastTask = source.Restart(TimeSpan.FromMilliseconds(10));
            }

            lastTask.Wait();
        }

        [Fact]
        public void StressTesting3()
        {
            var source = new DelayTaskFactory(DelayTaskCancellationMode.SetTaskResultToFalse);
            Task lastTask = null;

            for (int i = 0; i < 1000000; i++)
            {
                lastTask = source.Restart(TimeSpan.FromMilliseconds(10));
            }

            lastTask.Wait();
        }
    }
}