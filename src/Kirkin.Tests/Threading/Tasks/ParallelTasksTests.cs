using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Logging;
using Kirkin.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Kirkin.Tests.Threading.Tasks
{
    public class ParallelTasksTests
    {
        private readonly Logger Output;

        public ParallelTasksTests(ITestOutputHelper output)
        {
            Output = Logger
                .Create(output.WriteLine)
                .WithFormatters(EntryFormatter.TimestampNonEmptyEntries("HH:mm:ss.fff"));
        }

        private IEnumerable<Func<Task<TimeSpan>>> EnumerateFactories()
        {
            Output.Log("Yielding task 1 (100 ms).");
            yield return () => DoWorkAsync(TimeSpan.FromMilliseconds(100));
            Output.Log("Yielding task 2 (600 ms).");
            yield return () => DoWorkAsync(TimeSpan.FromMilliseconds(600));
            Output.Log("Yielding task 3 (300 ms).");
            yield return () => DoWorkAsync(TimeSpan.FromMilliseconds(300));
            Output.Log("Yielding task 4 (650 ms).");
            yield return () => DoWorkAsync(TimeSpan.FromMilliseconds(650)); // Slightly longer delay - otherwise likely to fail.
        }

        async Task<TimeSpan> DoWorkAsync(TimeSpan duration)
        {
            Output.Log($"Starting {duration.Milliseconds} ms task.");
            await Task.Delay(duration).ConfigureAwait(false);
            Output.Log($"Finished {duration.Milliseconds} ms task.");
            return duration;
        }

        [Fact]
        public async Task ParallelInvokeVoid()
        {
            await ParallelTasks.InvokeAsync(EnumerateFactories().Cast<Func<Task>>(), 3).ConfigureAwait(false);

            Output.Log("Done");
        }

        private static Task FaultedTask
        {
            get
            {
                TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();

                source.SetException(new ImmediateException());

                return source.Task;
            }
        }

        [Fact]
        public async Task ParallelInvokeOrderedResults()
        {
            TimeSpan[] results = await ParallelTasks.InvokeAsync(EnumerateFactories(), 3).ConfigureAwait(false);
            Assert.Equal(new[] { 100, 600, 300, 650 }, results.Select(r => (int)r.TotalMilliseconds));

            Output.Log("Done.");
        }

        [Fact]
        public async Task ParallelForAsync()
        {
            List<TimeSpan> results = new List<TimeSpan>();
            Func<Task<TimeSpan>>[] factories = EnumerateFactories().ToArray();

            await ParallelTasks.ForAsync(0, factories.Length, i => factories[i](), async completed => results.Add(await completed));

            Assert.Equal(new[] { 100, 300, 600, 650 }, results.Select(r => (int)r.TotalMilliseconds));
        }

        [Fact]
        public async Task ParallelForEachAsync()
        {
            List<TimeSpan> results = new List<TimeSpan>();

            await ParallelTasks.ForEachAsync(EnumerateFactories(), f => f(), async completed => results.Add(await completed));

            Assert.Equal(new[] { 100, 300, 600, 650 }, results.Select(r => (int)r.TotalMilliseconds));
        }

        [Fact]
        public async Task ParallelForEachAsyncLimited1()
        {
            List<TimeSpan> results = new List<TimeSpan>();
            ParallelTaskOptions options = new ParallelTaskOptions { MaxDegreeOfParallelism = 1 };

            await ParallelTasks.ForEachAsync(EnumerateFactories(), options, f => f(), async completed => results.Add(await completed));

            Assert.Equal(new[] { 100, 600, 300, 650 }, results.Select(r => (int)r.TotalMilliseconds));
        }

        [Fact]
        public async Task ParallelForEachAsyncLimited2()
        {
            List<TimeSpan> results = new List<TimeSpan>();
            ParallelTaskOptions options = new ParallelTaskOptions { MaxDegreeOfParallelism = 2 };

            await ParallelTasks.ForEachAsync(EnumerateFactories(), options, f => f(), async completed => results.Add(await completed));

            Assert.Equal(new[] { 100, 300, 600, 650 }, results.Select(r => (int)r.TotalMilliseconds));
        }

        [Fact]
        public async Task ParallelForEachAsyncCancellation()
        {
            List<TimeSpan> results = new List<TimeSpan>();
            ParallelTaskOptions options = new ParallelTaskOptions { CancellationToken = new CancellationTokenSource(0).Token };

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                ParallelTasks.ForEachAsync(EnumerateFactories(), options, f => f(), async completed => results.Add(await completed))
            );

            Assert.Equal(0, results.Count);

            options = new ParallelTaskOptions {
                CancellationToken = new CancellationTokenSource(250).Token,
                MaxDegreeOfParallelism = 2
            };

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                ParallelTasks.ForEachAsync(EnumerateFactories(), options, f => f(), async completed => results.Add(await completed))
            );

            Assert.Equal(new[] { 100, 300, 600 }, results.Select(r => (int)r.TotalMilliseconds));
        }

        [Fact]
        public async Task ParallelForEachFaultedWaitsUntilSafePoint()
        {
            int shortTasksStarted = 0;
            int longTasksStarted = 0;
            int synchronousTasksStarted = 0;

            Func<Task> startShort = async () =>
            {
                Interlocked.Increment(ref shortTasksStarted);
                await Task.Delay(10).ConfigureAwait(false);
                throw new ShortException();
            };

            Func<Task> startLong = async () =>
            {
                Interlocked.Increment(ref longTasksStarted);
                await Task.Delay(200).ConfigureAwait(false);
                throw new LongException(); // Different exception.
            };

            // Ensure tasks are not abandoned on deferred exception.
            ParallelTaskOptions options = new ParallelTaskOptions { MaxDegreeOfParallelism = 2 };
            Stopwatch sw = Stopwatch.StartNew();

            await Assert.ThrowsAsync<ShortException>(
                () => ParallelTasks.ForEachAsync(new[] { startShort, startLong, startShort }, options, f => f())
            );

            Assert.True(sw.ElapsedMilliseconds >= 100, "1: Long expected to complete before throw.");
            Assert.Equal(1, shortTasksStarted);
            Assert.Equal(1, longTasksStarted);

            Func<Task> throwSynchronously = () =>
            {
                Interlocked.Increment(ref synchronousTasksStarted);
                throw new ImmediateException();
            };

            // Ensure tasks are not abandoned even on immediate (synchronous) exception in the factory delegate.
            await Assert.ThrowsAsync<LongException>(
                () => ParallelTasks.ForEachAsync(new[] { startLong, throwSynchronously, startShort }, options, f => f())
            );

            Assert.Equal(1, shortTasksStarted);
            Assert.Equal(2, longTasksStarted);
            Assert.Equal(1, synchronousTasksStarted);

            // Ensure continuations fire after async exception in onCompleted.
            int onCompletedCount = 0;
            options = new ParallelTaskOptions { MaxDegreeOfParallelism = 4 };

            await Assert.ThrowsAsync<ShortException>(
                () => ParallelTasks.ForEachAsync(new[] { startShort, startLong, startLong, startLong, startShort }, options, f => f(), async completed =>
                {
                    Interlocked.Increment(ref onCompletedCount);
                    await Task.Yield();
                    await completed.ConfigureAwait(false); // Rethrow.
                })
            );

            Assert.Equal(2, shortTasksStarted);
            Assert.Equal(5, longTasksStarted);
            Assert.Equal(4, onCompletedCount);

            // Ensure continuations fire even after synchronous exception in onCompleted.
            await Assert.ThrowsAsync<ImmediateException>(
                () => ParallelTasks.ForEachAsync(new[] { startShort, startLong, startLong, startLong, startShort }, options, f => f(), completed =>
                {
                    Interlocked.Increment(ref onCompletedCount);
                    throw new ImmediateException(); // Synchronous.
                })
            );

            Assert.Equal(3, shortTasksStarted);
            Assert.Equal(8, longTasksStarted);
            Assert.Equal(8, onCompletedCount);
        }

        sealed class ShortException : InvalidOperationException
        {
        }

        sealed class LongException : InvalidOperationException
        {
        }

        sealed class ImmediateException : InvalidOperationException
        {
        }
    }
}