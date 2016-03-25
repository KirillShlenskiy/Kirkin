//#if !NET_40

//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Kirkin.Threading
//{
//    internal sealed class AsyncLock
//    {
//        private static readonly Task s_completedTask = Task.FromResult(false);
//        private static readonly Task s_canceledTask = CreateCanceledTask();

//        private static Task CreateCanceledTask()
//        {
//            var tcs = new TaskCompletionSource<bool>();

//            tcs.SetCanceled();

//            return tcs.Task;
//        }

//        private readonly Queue<TaskCompletionSource<bool>> Waiters = new Queue<TaskCompletionSource<bool>>();
//        private bool Taken = false;

//        public Task WaitAsync(CancellationToken ct = default(CancellationToken))
//        {
//            if (ct.IsCancellationRequested) {
//                return s_canceledTask; // Cancel synchronously.
//            }

//            lock (Waiters)
//            {
//                if (!Taken)
//                {
//                    Taken = true;

//                    return s_completedTask; // Complete synchronously.
//                }
//            }
//        }

//        public void Release()
//        {
//            lock (Waiters)
//            {
//                if (!Taken) {
//                    throw new InvalidOperationException("Attempting to release lock when no lock was taken.");
//                }

//                while (Waiters.Count != 0)
//                {
//                    // Allow the next non-canceled waiter to proceed.
//                    if (Waiters.Dequeue().TrySetResult(false)) {
//                        return;
//                    }
//                }
//            }
//        }

//        //public struct Releaser : IDisposable
//        //{
//        //    private readonly AsyncLock m_toRelease;

//        //    internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }

//        //    public void Dispose()
//        //    {
//        //        if (m_toRelease != null)
//        //            m_toRelease.Release();
//        //    }
//        //}
//    }
//}

//#endif