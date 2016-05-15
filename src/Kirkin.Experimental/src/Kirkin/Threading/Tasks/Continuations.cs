// This is *not* original work by Kirill Shlenskiy.
// It is taken in its entirety or derived from 
// CoreFX (https://github.com/dotnet/corefx).
// Original license below:

// The MIT License (MIT)

// Copyright (c) .NET Foundation and Contributors

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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