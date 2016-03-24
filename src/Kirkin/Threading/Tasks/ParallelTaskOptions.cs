using System;
using System.Threading;

namespace Kirkin.Threading.Tasks
{
    /// <summary>
    /// Stores options that configure the operation of
    /// methods on the <see cref="ParallelTasks"/> class.
    /// </summary>
    public struct ParallelTaskOptions
    {
        private int _maxDegreeOfParallelism;

        /// <summary>
        /// Gets or sets the <see cref="System.Threading.CancellationToken" />
        /// associated with this <see cref="ParallelTaskOptions"/> instance.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the maximum degree of parallelism enabled by this
        /// <see cref="ParallelTaskOptions"/> instance. Defaults to the number of
        /// processors as reported by <see cref="Environment.ProcessorCount"/>.
        /// </summary>
        public int MaxDegreeOfParallelism
        {
            get
            {
                if (_maxDegreeOfParallelism == 0) {
                    _maxDegreeOfParallelism = Environment.ProcessorCount;
                }

                return _maxDegreeOfParallelism;
            }
            set
            {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException("MaxDegreeOfParallelism");
                }

                _maxDegreeOfParallelism = value;
            }
        }
    }
}