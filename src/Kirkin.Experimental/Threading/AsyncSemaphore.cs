using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kirkin.Threading
{
    public sealed class AsyncSemaphore
    {
        private static readonly Task s_completedTask = Task.FromResult(true);

        private readonly Queue<TaskCompletionSource<bool>> Waiters = new Queue<TaskCompletionSource<bool>>();
        private int _count;

        public int CurrentCount
        {
            get
            {
                return _count;
            }
        }

        public int MaxCount { get; }

        public AsyncSemaphore(int initialCount)
            : this(initialCount, int.MaxValue)
        {
        }

        public AsyncSemaphore(int initialCount, int maxCount)
        {
            _count = initialCount;
            MaxCount = maxCount;
        }

        public Task WaitAsync()
        {
            lock (Waiters)
            {
                if (_count > 0)
                {
                    _count--;

                    return s_completedTask;
                }

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                Waiters.Enqueue(tcs);

                return tcs.Task;
            }
        }

        public void Release(int releaseCount = 1)
        {
            lock (Waiters)
            {
                if (Waiters.Count != 0)
                {
                    TaskCompletionSource<bool> tcs = Waiters.Dequeue();

                    tcs.SetResult(true);
                }
                else
                {
                    _count += releaseCount;
                }
            }
        }
    }
}