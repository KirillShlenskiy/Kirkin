using System;
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
        private readonly Dictionary<string, Operation> OperationsByName = new Dictionary<string, Operation>();

        /// <summary>
        /// All operations tracked by this profiler.
        /// </summary>
        public Operation[] Operations
        {
            get
            {
                return OperationsByName.Values.ToArray();
            }
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
        /// Returns an <see cref="IDisposable"/> scope which logs a time entry when disposed.
        /// </summary>
        public TimedScope Time(string operationName)
        {
            Operation operation;

            if (!OperationsByName.TryGetValue(operationName, out operation))
            {
                operation = new Operation(operationName);

                OperationsByName.Add(operationName, operation);
            }

            return new TimedScope(operation.Timespans);
        }

        public struct TimedScope : IDisposable
        {
            private readonly List<TimeSpan> Timespans;
            private readonly Stopwatch Stopwatch;

            internal TimedScope(List<TimeSpan> timespans)
            {
                Timespans = timespans;
                Stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (!Stopwatch.IsRunning) {
                    throw new InvalidOperationException("Multiple calls to TimedScope.Dispose detected.");
                }

                Stopwatch.Stop();
                Timespans.Add(Stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Operation timed by the mini profiler.
        /// </summary>
        public sealed class Operation
        {
            internal readonly List<TimeSpan> Timespans;

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
                Timespans = new List<TimeSpan>();
            }
        }
    }
}