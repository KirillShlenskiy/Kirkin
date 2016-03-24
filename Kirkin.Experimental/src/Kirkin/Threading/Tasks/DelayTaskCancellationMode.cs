namespace Kirkin.Threading.Tasks
{
    internal enum DelayTaskCancellationMode
    {
        /// <summary>
        /// Default behaviour: the task will transition to Canceled state.
        /// </summary>
        Cancel,

        /// <summary>
        /// Alternative behaviour: the task's Result will be set to false,
        /// meaning that it didn't really run to completion.
        /// </summary>
        SetTaskResultToFalse
    }
}