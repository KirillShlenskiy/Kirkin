using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        public struct TimedScope : IDisposable
        {
            private readonly Operation Operation;
            private readonly Stopwatch Stopwatch;

            internal TimedScope(Operation operation)
            {
                Operation = operation;
                Stopwatch = Stopwatch.StartNew();
            }

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
            private readonly ConcurrentBag<TimeSpan> Timespans;

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
                    return Timespans.Count;
                }
            }

            /// <summary>
            /// Gets the average operation duration.
            /// </summary>
            public TimeSpan AverageDuration
            {
                get
                {
                    return new TimeSpan((long)Timespans.Average(t => t.Ticks));
                }
            }

            /// <summary>
            /// Gets the maximum operation duration.
            /// </summary>
            public TimeSpan MaxDuration
            {
                get
                {
                    return Timespans.DefaultIfEmpty().Max();
                }
            }

            /// <summary>
            /// Gets the minimum operation duration.
            /// </summary>
            public TimeSpan MinDuration
            {
                get
                {
                    return Timespans.DefaultIfEmpty().Min();
                }
            }

            /// <summary>
            /// Gets the total operation duration.
            /// </summary>
            public TimeSpan TotalDuration
            {
                get
                {
                    return new TimeSpan(Timespans.Sum(t => t.Ticks));
                }
            }

            internal Operation(string name)
            {
                Name = name;
                Timespans = new ConcurrentBag<TimeSpan>();
            }

            internal void Add(TimeSpan timespan)
            {
                Timespans.Add(timespan);
            }

            public override string ToString()
            {
                return $"[{Name}] Count: {Count}, Total: {TotalDuration.TotalSeconds:0.00}s, Avg: {AverageDuration.TotalSeconds:0.00}s, Min: {MinDuration.TotalSeconds:0.00}s, Max: {MaxDuration.TotalSeconds:0.00}s";
            }
        }
    }
}