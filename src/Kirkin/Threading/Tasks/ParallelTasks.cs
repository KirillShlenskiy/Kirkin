#if !NET_40

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Kirkin.Collections.Generic;

namespace Kirkin.Threading.Tasks
{
    /// <summary>
    /// Provides rough async-aware equivalents of <see cref="Parallel"/> methods.
    /// </summary>
    public static class ParallelTasks
    {
        #region Public API

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <returns>Task representing the completion of all operations.</returns>
        public static Task ForAsync(int fromInclusive, int toExclusive, Func<int, Task> body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            return ForAsyncWorker(fromInclusive, toExclusive, default(ParallelTaskOptions), body, null);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">Parallel excecution options.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <returns>Task representing the completion of all operations.</returns>
        public static Task ForAsync(int fromInclusive, int toExclusive, ParallelTaskOptions parallelOptions, Func<int, Task> body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            return ForAsyncWorker(fromInclusive, toExclusive, parallelOptions, body, null);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="onCompleted">
        /// Async continuation to be invoked when a task has completed. These will execute
        /// in random order on the thread pool, but are guaranteed to run one at a time.
        /// If a body task is started, its completion delegate is guaranteed to be invoked.
        /// </param>
        /// <returns>Task representing the completion of all operations.</returns>
        public static Task ForAsync<TTask>(int fromInclusive, int toExclusive, Func<int, TTask> body, Func<TTask, Task> onCompleted)
            where TTask : Task
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (onCompleted == null) throw new ArgumentNullException(nameof(onCompleted));
            return ForAsyncWorker(fromInclusive, toExclusive, default(ParallelTaskOptions), body, onCompleted);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">Parallel excecution options.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        /// <param name="onCompleted">
        /// Async continuation to be invoked when a task has completed. These will execute
        /// in random order on the thread pool, but are guaranteed to run one at a time.
        /// If a body task is started, its completion delegate is guaranteed to be invoked.
        /// </param>
        /// <returns>Task representing the completion of all operations.</returns>
        public static Task ForAsync<TTask>(int fromInclusive, int toExclusive, ParallelTaskOptions parallelOptions, Func<int, TTask> body, Func<TTask, Task> onCompleted)
            where TTask : Task
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (onCompleted == null) throw new ArgumentNullException(nameof(onCompleted));
            return ForAsyncWorker(fromInclusive, toExclusive, parallelOptions, body, onCompleted);
        }

        /// <summary>
        /// Invokes the given task selector on each element in the
        /// source collection and runs the resulting tasks in parallel.
        /// </summary>
        /// <param name="source">Source collection.</param>
        /// <param name="body">Task-producing delegate.</param>
        /// <returns>Task representing the completion of all operations.</returns>
        public static Task ForEachAsync<T>(IEnumerable<T> source, Func<T, Task> body)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (body == null) throw new ArgumentNullException(nameof(body));
            return ForEachAsyncWorker(source, default(ParallelTaskOptions), body, null);
        }

        /// <summary>
        /// Invokes the given task selector on each element in the
        /// source collection and runs the resulting tasks in parallel.
        /// </summary>
        /// <param name="source">Source collection.</param>
        /// <param name="parallelOptions">Parallel excecution options.</param>
        /// <param name="body">Task-producing delegate.</param>
        /// <returns>Task representing the completion of all operations.</returns>
        public static Task ForEachAsync<T>(IEnumerable<T> source, ParallelTaskOptions parallelOptions, Func<T, Task> body)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (body == null) throw new ArgumentNullException(nameof(body));
            return ForEachAsyncWorker(source, parallelOptions, body, null);
        }

        /// <summary>
        /// Invokes the given task selector on each element in the
        /// source collection and runs the resulting tasks in parallel.
        /// </summary>
        /// <param name="source">Source collection.</param>
        /// <param name="body">Task-producing delegate.</param>
        /// <param name="onCompleted">
        /// Async continuation to be invoked when a task has completed. These will execute
        /// in random order on the thread pool, but are guaranteed to run one at a time.
        /// If a body task is started, its completion delegate is guaranteed to be invoked.
        /// </param>
        /// <returns>Task representing the completion of all operations.</returns>
        public static Task ForEachAsync<T, TTask>(IEnumerable<T> source, Func<T, TTask> body, Func<TTask, Task> onCompleted)
            where TTask : Task
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (onCompleted == null) throw new ArgumentNullException(nameof(onCompleted));
            return ForEachAsyncWorker(source, default(ParallelTaskOptions), body, onCompleted);
        }

        /// <summary>
        /// Invokes the given task selector on each element in the
        /// source collection and runs the resulting tasks in parallel.
        /// </summary>
        /// <param name="source">Source collection.</param>
        /// <param name="parallelOptions">Parallel excecution options.</param>
        /// <param name="body">Task-producing delegate.</param>
        /// <param name="onCompleted">
        /// Async continuation to be invoked when a task has completed. These will execute
        /// in random order on the thread pool, but are guaranteed to run one at a time.
        /// If a body task is started, its completion delegate is guaranteed to be invoked.
        /// </param>
        /// <returns>Task representing the completion of all operations.</returns>
        public static Task ForEachAsync<T, TTask>(IEnumerable<T> source, ParallelTaskOptions parallelOptions, Func<T, TTask> body, Func<TTask, Task> onCompleted)
            where TTask : Task
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (onCompleted == null) throw new ArgumentNullException(nameof(onCompleted));
            return ForEachAsyncWorker(source, parallelOptions, body, onCompleted);
        }

        /// <summary>
        /// Executes the given async delegates in parallel,
        /// up to the given maximum degree of parallelism.
        /// </summary>
        public static Task InvokeAsync(IEnumerable<Func<Task>> taskFactories, int maxDegreeOfParallelism)
        {
            if (taskFactories == null) throw new ArgumentNullException(nameof(taskFactories));
            if (maxDegreeOfParallelism <= 0) throw new ArgumentException(nameof(maxDegreeOfParallelism));
            return ForEachAsyncWorker(taskFactories, new ParallelTaskOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, f => f(), null);
        }

        /// <summary>
        /// Executes the given async delegates in parallel,
        /// up to the given maximum degree of parallelism.
        /// Returns an array containing ordered task results.
        /// </summary>
        public static async Task<T[]> InvokeAsync<T>(IEnumerable<Func<Task<T>>> taskFactories, int maxDegreeOfParallelism)
        {
            if (taskFactories == null) throw new ArgumentNullException(nameof(taskFactories));
            if (maxDegreeOfParallelism <= 0) throw new ArgumentException(nameof(maxDegreeOfParallelism));

            Func<Task<T>>[] factories = taskFactories.ToArray();

            if (factories.Length == 0) {
                return Array<T>.Empty;
            }

            // Key is index, Value is task factory.
            IEnumerable<KeyValuePair<int, Func<Task<T>>>> indexedTaskFactories = taskFactories.Select((f, i) => new KeyValuePair<int, Func<Task<T>>>(i, f));
            T[] results = new T[factories.Length];

            await ForEachAsyncWorker(
                indexedTaskFactories,
                new ParallelTaskOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                async indexedTaskFactory => results[indexedTaskFactory.Key] = await indexedTaskFactory.Value().ConfigureAwait(false), // Thread-safe as each Task writes to its own index.
                null
            ).ConfigureAwait(false);

            return results;
        }

        #endregion

        #region Implementation

        private static readonly Task s_completedTask = Task.FromResult(true);

        private static Task ForAsyncWorker<TTask>(int fromInclusive, int toExclusive, ParallelTaskOptions parallelOptions, Func<int, TTask> body, Func<TTask, Task> onCompleted)
            where TTask : Task
        {
            return fromInclusive >= toExclusive
                ? s_completedTask
                : ForEachAsyncWorker(Enumerable.Range(fromInclusive, toExclusive - fromInclusive), parallelOptions, body, onCompleted);
        }

        // Notes re method signature choice: I could have used IEnumerable<Func<TTask>> as
        // the source, but this would require the allocation of a potentially significant number
        // of TTask-producing delegates. Instead, I'm using IEnumerable<T> and a single Func<T, TTask>
        // factory. This leads to better resource usage at the expense of a less friendly signature.
        private static async Task ForEachAsyncWorker<T, TTask>(IEnumerable<T> source, ParallelTaskOptions parallelOptions, Func<T, TTask> body, Func<TTask, Task> onCompleted)
            where TTask : Task
        {
            parallelOptions.CancellationToken.ThrowIfCancellationRequested();

            using (ConsumingQueue<T> queue = new ConsumingQueue<T>(source))
            {
                if (queue.IsEmpty) {
                    return;
                }

                List<TTask> tasksInFlight = new List<TTask>(parallelOptions.MaxDegreeOfParallelism);
                List<Task> faultedTasks = null;

                try
                {
                    do
                    {
                        while (tasksInFlight.Count < parallelOptions.MaxDegreeOfParallelism &&
                               !queue.IsEmpty &&
                               !parallelOptions.CancellationToken.IsCancellationRequested &&
                               faultedTasks == null)
                        {
                            TTask bodyTask = body(queue.Dequeue());

                            tasksInFlight.Add(bodyTask);
                        }

                        TTask completedBodyTask = (TTask)await Task.WhenAny(tasksInFlight).ConfigureAwait(false);

                        Task fullCompletion = (onCompleted == null) ? completedBodyTask
                            // onCompleted may handle exceptions from the completed Task if it is faulted. The reason for wrapping
                            // onCompleted in Task.Run is so that we have a Task instance to add to faultedTasks in the event of an
                            // exception inside the synchronous portion of onCompleted. CancellationToken intentionally omitted.
                            : Task.Run(() => onCompleted(completedBodyTask));

                        try
                        {
                            await fullCompletion.ConfigureAwait(false);
                        }
                        catch
                        {
                            if (faultedTasks == null) faultedTasks = new List<Task>();

                            // Exception will be rethrown inside "finally" if necessary.
                            faultedTasks.Add(fullCompletion);
                        }

                        tasksInFlight.Remove(completedBodyTask);
                    }
                    while (tasksInFlight.Count != 0 ||
                          (!queue.IsEmpty && !parallelOptions.CancellationToken.IsCancellationRequested && faultedTasks == null));
                }
                finally
                {
                    if (faultedTasks != null)
                    {
                        if (tasksInFlight.Count != 0) {
                            throw new InvalidOperationException("Unobserved tasks in flight detected.");
                        }

                        await Task.WhenAll(faultedTasks).ConfigureAwait(false);
                    }
                    else if (tasksInFlight.Count != 0)
                    {
                        // Tasks in flight need to be checked in case of
                        // exceptions synchronously thrown by body delegates.
                        await Task.WhenAll(tasksInFlight).ConfigureAwait(false);
                    }
                }

                // Allow cancellation only if we haven't completed. Otherwise it might be too late.
                if (!queue.IsEmpty) {
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        class ConsumingQueue<T> : IDisposable
        {
            private IEnumerator<T> enumerator;
            private Box _next;

            private Box Next
            {
                get
                {
                    if (!_next.HasValue && enumerator != null)
                    {
                        if (enumerator.MoveNext())
                        {
                            _next = new Box(enumerator.Current);
                        }
                        else
                        {
                            enumerator.Dispose();
                            enumerator = null;
                        }
                    }

                    return _next;
                }
            }

            public bool IsEmpty
            {
                get
                {
                    return !Next.HasValue;
                }
            }

            internal ConsumingQueue(IEnumerable<T> collection)
            {
                enumerator = collection.GetEnumerator();
                _next = default(Box);
            }

            public T Dequeue()
            {
                T value = Next.Value;
                _next = default(Box);
                return value;
            }

            public void Dispose()
            {
                if (enumerator != null) {
                    enumerator.Dispose();
                }
            }

            struct Box
            {
                private readonly bool _hasValue;
                private readonly T _value;

                public bool HasValue
                {
                    get
                    {
                        return _hasValue;
                    }
                }

                public T Value
                {
                    get
                    {
                        if (!_hasValue) {
                            throw new InvalidOperationException("Value is undefined when HasValue is false.");
                        }

                        return _value;
                    }
                }

                internal Box(T value)
                {
                    _value = value;
                    _hasValue = true;
                }
            }
        }

        #endregion
    }
}

#endif