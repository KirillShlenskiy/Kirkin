using System;

#if !NET_40
using System.Threading.Tasks;
#endif

namespace Kirkin.Functional
{
    /// <summary>
    /// Execution block factory type.
    /// </summary>
    internal static class Executable
    {
        /// <summary>
        /// Creates an executable which invokes the given action when executed.
        /// </summary>
        public static IExecutable Create(Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            return new ActionExecutable(action);
        }

        sealed class ActionExecutable : IExecutable
        {
            private readonly Action Action;

            internal ActionExecutable(Action action)
            {
                Action = action;
            }

            public void Execute()
            {
                Action();
            }
        }

#if !NET_40
        /// <summary>
        /// Creates an executable which invokes the given asynchronous action when executed.
        /// </summary>
        public static IAsyncExecutable CreateAsync(Func<Task> taskFactory)
        {
            if (taskFactory == null) throw new ArgumentNullException("taskFactory");

            return new AsyncExecutable(taskFactory);
        }

        sealed class AsyncExecutable : IAsyncExecutable
        {
            private readonly Func<Task> TaskFactory;

            internal AsyncExecutable(Func<Task> taskFactory)
            {
                TaskFactory = taskFactory;
            }

            public Task ExecuteAsync()
            {
                return TaskFactory();
            }
        }
#endif
    }
}