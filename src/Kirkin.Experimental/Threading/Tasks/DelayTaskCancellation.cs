using System;
using System.Threading.Tasks;

namespace Kirkin.Threading.Tasks
{
    internal static class DelayTaskCancellation
    {
        internal static void Apply(TaskCompletionSource<bool> taskCompletionSource, DelayTaskCancellationMode mode)
        {
            switch (mode)
            {
                case DelayTaskCancellationMode.Cancel:
                    taskCompletionSource.SetCanceled();
                    break;

                case DelayTaskCancellationMode.SetTaskResultToFalse:
                    taskCompletionSource.SetResult(false);
                    break;

                default:
                    throw new InvalidOperationException("Unhandled cancellation mode.");
            }
        }
    }
}