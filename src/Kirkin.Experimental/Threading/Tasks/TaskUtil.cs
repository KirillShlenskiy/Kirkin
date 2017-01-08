using System;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Linq.Expressions;

namespace Kirkin.Threading.Tasks
{
    /// <summary>
    /// Common <see cref="Task"/> helpers.
    /// </summary>
    public static class TaskUtil
    {
        private static readonly Task s_completedTask = CreateCompletedTask();

        /// <summary>
        /// Shared instance of an already completed task.
        /// </summary>
        public static Task CompletedTask
        {
            get
            {
                return s_completedTask;
            }
        }

        /// <summary>
        /// Creates a <see cref="Task"/> in a completed or canceled state.
        /// </summary>
        public static Task CreateCompletedTask(bool canceled = false, TaskCreationOptions creationOptions = TaskCreationOptions.None, CancellationToken ct = default(CancellationToken))
        {
            return CompletedTaskFactory.Factory(canceled, creationOptions, ct);
        }

        /// <summary>
        /// Creates a <see cref="Task"/> in a completed or canceled state, with the given result.
        /// </summary>
        public static Task<TResult> CreateCompletedTask<TResult>(bool canceled = false, TResult result = default(TResult), TaskCreationOptions creationOptions = TaskCreationOptions.None, CancellationToken ct = default(CancellationToken))
        {
            return CompletedTaskFactory<TResult>.Factory(canceled, result, creationOptions, ct);
        }

        static class CompletedTaskFactory
        {
            public static readonly Func<bool, TaskCreationOptions, CancellationToken, Task> Factory = MemberExpressions
                .Constructor<Task>()
                .WithParameters<bool, TaskCreationOptions, CancellationToken>(nonPublic: true)
                .Compile();
        }

        static class CompletedTaskFactory<TResult>
        {
            public static readonly Func<bool, TResult, TaskCreationOptions, CancellationToken, Task<TResult>> Factory = MemberExpressions
                .Constructor<Task<TResult>>()
                .WithParameters<bool, TResult, TaskCreationOptions, CancellationToken>(nonPublic: true)
                .Compile();
        }
    }
}