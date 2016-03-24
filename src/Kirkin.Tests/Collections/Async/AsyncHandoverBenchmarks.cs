using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Xunit;

using Kirkin.Collections.Async;

namespace Kirkin.Tests.Collections.Async
{
    public class AsyncHandoverBenchmarks
    {
        public AsyncHandoverBenchmarks()
        {
            this.__AsyncHandoverWarmup();
            this.__BlockingCollectionWarmup();
            this.__BufferBlockWarmup();
        }

        public void __AsyncHandoverWarmup()
        {
            var queue = new AsyncHandover<int>();

            queue.Add(1);
            queue.Add(2);
            queue.Add(3);
            queue.CompleteAdding();
        }

        public void __BufferBlockWarmup()
        {
            var ab = new BufferBlock<int>();

            ab.Post(1);
            ab.Post(2);
            ab.Post(3);
            ab.Complete();
        }

        public void __BlockingCollectionWarmup()
        {
            using (var collection = new BlockingCollection<int>())
            {
                collection.Add(1);
                collection.Add(2);
                collection.Add(3);
                collection.CompleteAdding();
            }
        }

        [Fact]
        public async Task SlowProducerAsyncHandoverBenchmark()
        {
            var queue = new AsyncHandover<int>();

            var producer = Task.Run(async () =>
            {
                try
                {
                    for (var i = 0; i < 25; i++)
                    {
                        await Task.Delay(10).ConfigureAwait(false);

                        queue.Add(i);
                    }
                }
                finally
                {
                    queue.CompleteAdding();
                }
            });

            var consumer = Task.Run(async () =>
            {
                TakeResult<int> result;

                while ((result = await queue.TryTakeAsync().ConfigureAwait(false)).Success) {
                    Debug.Print(result.Value.ToString());
                }
            });

            await Task.WhenAll(producer, consumer).ConfigureAwait(false);
        }

        [Fact]
        public async Task SlowProducerBlockingCollectionBenchmark()
        {
            var queue = new BlockingCollection<int>();

            var producer = Task.Run(async () =>
            {
                try
                {
                    for (var i = 0; i < 25; i++)
                    {
                        await Task.Delay(10).ConfigureAwait(false);

                        queue.Add(i);
                    }
                }
                finally
                {
                    queue.CompleteAdding();
                }
            });

            var consumer = Task.Run(() =>
            {
                foreach (var i in queue.GetConsumingEnumerable())
                {
                    Debug.Print(i.ToString());
                }
            });

            await Task.WhenAll(producer, consumer).ConfigureAwait(false);
        }

        [Fact]
        public async Task SlowProducerBufferBlockBenchmark()
        {
            var queue = new BufferBlock<int>();

            var producer = Task.Run(async () =>
            {
                try
                {
                    for (var i = 0; i < 25; i++)
                    {
                        await Task.Delay(10).ConfigureAwait(false);

                        queue.Post(i);
                    }
                }
                finally
                {
                    queue.Complete();
                }
            });

            var consumer = Task.Run(async () =>
            {
                while (await queue.OutputAvailableAsync().ConfigureAwait(false))
                {
                    var i = queue.Receive();

                    Debug.Print(i.ToString());
                }
            });

            await Task.WhenAll(producer, consumer).ConfigureAwait(false);
        }

        [Fact]
        public async Task SlowConsumerAsyncHandoverBenchmark()
        {
            var queue = new AsyncHandover<int>();

            var producer = Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < 25; i++)
                    {
                        queue.Add(i);
                    }
                }
                finally
                {
                    queue.CompleteAdding();
                }
            });

            var consumer = Task.Run(async () =>
            {
                TakeResult<int> result;

                while ((result = await queue.TryTakeAsync().ConfigureAwait(false)).Success)
                {
                    await Task.Delay(10).ConfigureAwait(false);

                    Debug.Print(result.Value.ToString());
                }
            });

            await Task.WhenAll(producer, consumer).ConfigureAwait(false);
        }

        [Fact]
        public async Task SlowConsumerBlockingCollectionBenchmark()
        {
            var queue = new BlockingCollection<int>();

            var producer = Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < 25; i++)
                    {
                        queue.Add(i);
                    }
                }
                finally
                {
                    queue.CompleteAdding();
                }
            });

            var consumer = Task.Run(async () =>
            {
                foreach (var i in queue.GetConsumingEnumerable())
                {
                    await Task.Delay(10).ConfigureAwait(false);

                    Debug.Print(i.ToString());
                }
            });

            await Task.WhenAll(producer, consumer).ConfigureAwait(false);
        }

        [Fact]
        public async Task SlowConsumerBufferBlockBenchmark()
        {
            var queue = new BufferBlock<int>();

            var producer = Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < 25; i++)
                    {
                        queue.Post(i);
                    }
                }
                finally
                {
                    queue.Complete();
                }
            });

            var consumer = Task.Run(async () =>
            {
                while (await queue.OutputAvailableAsync().ConfigureAwait(false))
                {
                    var i = queue.Receive();

                    await Task.Delay(10).ConfigureAwait(false);

                    Debug.Print(i.ToString());
                }
            });

            await Task.WhenAll(producer, queue.Completion).ConfigureAwait(false);
        }

        [Fact]
        public async Task LargeNumberOfItemsHandoverBenchmark()
        {
            var collection = new AsyncHandover<int>(new ConcurrentQueue<int>(Enumerable.Range(1, 3000000)));

            collection.CompleteAdding();

            TakeResult<int> result;

            while ((result = await collection.TryTakeAsync().ConfigureAwait(false)).Success)
            {
                // Do something with result.
            }
        }
    }
}
