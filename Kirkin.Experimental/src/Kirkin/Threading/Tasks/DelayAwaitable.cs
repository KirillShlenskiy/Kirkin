using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Kirkin.Threading.Tasks
{
    internal struct DelayAwaitable
    {
        public TimeSpan CompleteAfter { get; }

        public DelayAwaitable(int millisecondsDelay)
        {
            CompleteAfter = TimeSpan.FromMilliseconds(millisecondsDelay);
        }

        public DelayAwaitable(TimeSpan completeAfter)
        {
            CompleteAfter = completeAfter;
        }

        public DelayAwaiter GetAwaiter()
        {
            return new DelayAwaiter(CompleteAfter);
        }

        public struct DelayAwaiter : ICriticalNotifyCompletion
        {
            private readonly TimeSpan CompleteAfter;

            public bool IsCompleted
            {
                get
                {
                    return CompleteAfter == TimeSpan.Zero;
                }
            }

            internal DelayAwaiter(TimeSpan completeAfter)
            {
                CompleteAfter = completeAfter;
            }

            public void GetResult()
            {
                // No op. Required by compiler.
            }

            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException("DIAG: OnCompleted");
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                StartTimer(CompleteAfter, () => Continuations.QueueContinuation(continuation, false));
            }

            private void StartTimer(TimeSpan completeAfter, Action action)
            {
                if (completeAfter == TimeSpan.Zero)
                {
                    action();
                }
                else
                {
                    int dueTime = (int)completeAfter.TotalMilliseconds;
                    Timer timer = null; // Self-disposing.

                    timer = new Timer(
                        new TimerCallback(_ =>
                        {
                            try
                            {
                                action();
                            }
                            finally
                            {
                                timer.Dispose();
                            }
                        })
                        , null
                        , dueTime
                        , Timeout.Infinite
                    );
                }
            }
        }
    }
}