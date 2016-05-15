using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Kirkin.Threading.Tasks
{
    internal class AwaitableCompletionSource<T>
    {
        private readonly List<Action> Callbacks = new List<Action>();

        private bool IsCompleted;
        private T Result;

        public Awaiter GetAwaiter()
        {
            return new Awaiter(this);
        }

        public void SetResult(T result)
        {
            Result = result;
            IsCompleted = true;

            foreach (Action callback in Callbacks) {
                Continuations.QueueContinuation(callback, true);
            }
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly AwaitableCompletionSource<T> Source;

            internal Awaiter(AwaitableCompletionSource<T> source)
            {
                Source = source;
            }

            public bool IsCompleted
            {
                get
                {
                    return Source.IsCompleted;
                }
            }

            public T GetResult()
            {
                return Source.Result;
            }

            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Source.Callbacks.Add(continuation);
            }
        }
    }
}