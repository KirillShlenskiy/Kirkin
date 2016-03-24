using System.Threading;

namespace Kirkin.Threading
{
    /// <summary>
    /// Encapsulates an atomically read and written value.
    /// </summary>
    public sealed class Atomic<T>
        where T : struct
    {
        // Immutable state.
        private readonly object Lock = new object();

        // Mutable state.
        private volatile int Version; // incremented with each write
        private T _value;

        /// <summary>
        /// Gets or sets the value of this instance.
        /// </summary>
        public T Value
        {
            get
            {
                T value;
                int beforeVersion, afterVersion = Version; // Volatile ensures we are not reordered with _value read.

                do
                {
                    beforeVersion = afterVersion;

                    while (beforeVersion % 2 != 0)
                    {
                        // Wait until write finishes and update version.
                        lock (Lock) {
                            beforeVersion = Version; // Monitor's fence prevents reordering with _value read.
                        }
                    }

                    value = _value;
                    Thread.MemoryBarrier();
                    afterVersion = Version;
                }
                while (beforeVersion != afterVersion);

                return value;
            }
            set
            {
                lock (Lock)
                {
                    // No overflow checks required.
                    // int.MaxValue is even.
                    // int.MinValue is odd.
                    // -1 is even.
                    // 0 is odd.
                    Interlocked.Increment(ref Version); // Full fence. Version odd: write in progress.

                    _value = value;

                    Interlocked.Increment(ref Version); // Full fence. Version even: write complete.
                }
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="Atomic{T}"/> with a default initial value.
        /// </summary>
        public Atomic()
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="Atomic{T}"/> with the given initial value.
        /// </summary>
        public Atomic(T value)
        {
            _value = value;
        }
    }
}