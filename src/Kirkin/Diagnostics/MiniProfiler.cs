using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Lightweight performance profiler.
    /// </summary>
    public sealed class MiniProfiler
    {
        /// <summary>
        /// Durations by operation name.
        /// </summary>
        private readonly ConcurrentDictionary<string, Operation> OperationsByName = new ConcurrentDictionary<string, Operation>();
        private readonly ConcurrentDictionary<string, TimedScope> Scopes = new ConcurrentDictionary<string, TimedScope>();

        /// <summary>
        /// All operations tracked by this profiler.
        /// </summary>
        public ICollection<Operation> Operations
        {
            get
            {
                return OperationsByName.Values;
            }
        }

        /// <summary>
        /// Starts timing the operation with the given name.
        /// </summary>
        public void BeginTime(string operationName)
        {
            TimedScope scope = Time(operationName);

            if (!Scopes.TryAdd(operationName, scope)) {
                throw new InvalidOperationException($"Another '{operationName}' scope is already open and must be closed before you can call {nameof(BeginTime)} again.");
            }
        }

        /// <summary>
        /// Stops timing the operation with the given name.
        /// </summary>
        public void EndTime(string operationName)
        {
            TimedScope scope;

            if (!Scopes.TryRemove(operationName, out scope)) {
                throw new InvalidOperationException($"No open scope matching operation name '{operationName}' found.");
            }

            scope.Dispose();
        }

        /// <summary>
        /// Times the given action.
        /// </summary>
        public void Time(string operationName, Action action)
        {
            using (Time(operationName)) {
                action();
            }
        }

        /// <summary>
        /// Times the given function call.
        /// </summary>
        public T Time<T>(string operationName, Func<T> func)
        {
            using (Time(operationName)) {
                return func();
            }
        }

        /// <summary>
        /// Returns an <see cref="IDisposable"/> scope which logs a time entry when disposed.
        /// </summary>
        public TimedScope Time(string operationName)
        {
            Operation operation;

            if (!OperationsByName.TryGetValue(operationName, out operation)) {
                operation = OperationsByName.GetOrAdd(operationName, new Operation(operationName));
            }

            return new TimedScope(operation);
        }

        /// <summary>
        /// Disposable scope involved in timing an operation.
        /// When the scope is disposed the operation is marked as completed.
        /// </summary>
        public struct TimedScope : IDisposable
        {
            private readonly Operation Operation;
            private readonly Stopwatch Stopwatch;

            internal TimedScope(Operation operation)
            {
                Operation = operation;
                Stopwatch = Stopwatch.StartNew();
            }

            /// <summary>
            /// Marks the operation as completed.
            /// </summary>
            public void Dispose()
            {
                if (!Stopwatch.IsRunning) {
                    throw new InvalidOperationException("Multiple calls to TimedScope.Dispose detected.");
                }

                Stopwatch.Stop();
                Operation.Add(Stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Operation timed by the mini profiler.
        /// </summary>
        public sealed class Operation
        {
            private long _sum;
            private int _count;
            private long _min;
            private long _max;

            /// <summary>
            /// Name of the timed operation.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the number of times the operation was invoked.
            /// </summary>
            public int Count
            {
                get
                {
                    return _count;
                }
            }

            /// <summary>
            /// Gets the average operation duration.
            /// </summary>
            public TimeSpan AverageDuration
            {
                get
                {
                    return new TimeSpan((long)((double)_sum / _count));
                }
            }

            /// <summary>
            /// Gets the maximum operation duration.
            /// </summary>
            public TimeSpan MaxDuration
            {
                get
                {
                    return new TimeSpan(_max);
                }
            }

            /// <summary>
            /// Gets the minimum operation duration.
            /// </summary>
            public TimeSpan MinDuration
            {
                get
                {
                    return new TimeSpan(_min);
                }
            }

            /// <summary>
            /// Gets the total operation duration.
            /// </summary>
            public TimeSpan TotalDuration
            {
                get
                {
                    return new TimeSpan(_sum);
                }
            }

            internal Operation(string name)
            {
                Name = name;
            }

            internal void Add(TimeSpan timespan)
            {
                lock (this)
                {
                    _count++;
                    _sum += timespan.Ticks;

                    if (timespan.Ticks > _max) _max = timespan.Ticks;
                    if (timespan.Ticks < _min || _min == 0) _min = timespan.Ticks;
                }
            }

            /// <summary>
            /// Returns the operation stats in human-readable form.
            /// </summary>
            public override string ToString()
            {
                return $"[{Name}] Count: {Count}, Total: {TotalDuration.TotalSeconds:0.00}s, Avg: {AverageDuration.TotalSeconds:0.00}s, Min: {MinDuration.TotalSeconds:0.00}s, Max: {MaxDuration.TotalSeconds:0.00}s";
            }
        }
    }
}