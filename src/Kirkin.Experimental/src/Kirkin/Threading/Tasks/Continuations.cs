using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Threading.Tasks
{
    internal static class Continuations
    {
        private static readonly WaitCallback s_waitCallbackRunAction = state => ((Action)state)();
        private static readonly SendOrPostCallback s_sendOrPostCallbackRunAction = state => ((Action)state)();

        private static readonly Func<SynchronizationContext> s_syncContextNoFlowGetter = (Func<SynchronizationContext>)Delegate.CreateDelegate(
            typeof(Func<SynchronizationContext>),
            typeof(SynchronizationContext)
                .GetProperty("CurrentNoFlow", BindingFlags.Static | BindingFlags.NonPublic)
                .GetGetMethod(nonPublic: true)
        );

        private static SynchronizationContext SyncContextCurrentNoFlow
        {
            get
            {
                return s_syncContextNoFlowGetter();
            }
        }

        public static void QueueContinuation(Action continuation, bool flowContext)
        {
            if (continuation == null) throw new ArgumentNullException("continuation");

            // Get the current SynchronizationContext, and if there is one,
            // post the continuation to it. However, treat the base type as
            // if there wasn't a SynchronizationContext, since that's what it
            // logically represents.
            SynchronizationContext syncCtx = SyncContextCurrentNoFlow;

            if (syncCtx != null && syncCtx.GetType() != typeof(SynchronizationContext))
            {
                syncCtx.Post(s_sendOrPostCallbackRunAction, continuation);
            }
            else
            {
                // If we're targeting the default scheduler, queue to the thread pool, so that we go into the global
                // queue.  As we're going into the global queue, we might as well use QUWI, which for the global queue is
                // just a tad faster than task, due to a smaller object getting allocated and less work on the execution path.
                TaskScheduler scheduler = TaskScheduler.Current;

                if (scheduler == TaskScheduler.Default)
                {
                    if (flowContext)
                    {
                        ThreadPool.QueueUserWorkItem(s_waitCallbackRunAction, continuation);
                    }
                    else
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(s_waitCallbackRunAction, continuation);
                    }
                }
                else
                {
                    // We're targeting a custom scheduler, so queue a task.
                    Task.Factory.StartNew(continuation, default(CancellationToken), TaskCreationOptions.PreferFairness, scheduler);
                }
            }
        }
    }
}