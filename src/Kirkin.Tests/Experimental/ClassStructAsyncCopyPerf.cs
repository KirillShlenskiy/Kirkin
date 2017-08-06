using System.Threading.Tasks;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class ClassStructAsyncCopyPerf
    {
        private static readonly Task s_completedTask = Task.FromResult(true);

        const int Iterations = 5000000;

        [Test]
        public void SyncStruct()
        {
            for (int i = 0; i < Iterations; i++) {
                new TcsWrapperStruct(new TaskCompletionSource<bool>()).Set();
            }
        }

        [Test]
        public void SyncClass()
        {
            for (int i = 0; i < Iterations; i++) {
                new TcsWrapperClass(new TaskCompletionSource<bool>()).Set();
            }
        }

        [Test]
        public async Task AsyncStruct()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var wrapper = new TcsWrapperStruct(new TaskCompletionSource<bool>());

                await wrapper.Set().ConfigureAwait(false);
            }
        }

        [Test]
        public async Task AsyncClass()
        {
            for (int i = 0; i < Iterations; i++)
            {
                var wrapper = new TcsWrapperClass(new TaskCompletionSource<bool>());

                await wrapper.Set().ConfigureAwait(false);
            }
        }

        sealed class TcsWrapperClass
        {
            private readonly TaskCompletionSource<bool> _tcs;

            public TcsWrapperClass(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public Task Set()
            {
                _tcs.TrySetResult(true);

                return _tcs.Task;
            }
        }

        struct TcsWrapperStruct
        {
            private readonly TaskCompletionSource<bool> _tcs;

            public TcsWrapperStruct(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public Task Set()
            {
                _tcs.TrySetResult(true);

                return _tcs.Task;
            }
        }
    }
}