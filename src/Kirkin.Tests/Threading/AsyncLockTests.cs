using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Threading;

using NUnit.Framework;

namespace Kirkin.Tests.Threading
{
    public class AsyncLockTests
    {
        [Test]
        public void MutexTests()
        {
            AsyncLock l = new AsyncLock();
            int badInt = int.MaxValue - 1;
            Task[] tasks = new Task[10];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    using (AsyncLockReleaser releaser = await l.EnterAsync().ConfigureAwait(false))
                    {
                        checked
                        {
                            badInt++;
                            Thread.Sleep(100);
                            badInt--;
                        }
                    }
                });
            }

            Task.WaitAll(tasks);

            Assert.AreEqual(int.MaxValue - 1, badInt);
        }

        [Test]
        public void SequentialCompletionTests()
        {
            AsyncLock l = new AsyncLock();
            Task[] tasks = new Task[10];
            List<int> ints = new List<int>();

            for (int i = 0; i < tasks.Length; i++)
            {
                int _i = i;
                Task<AsyncLockReleaser> releaserTask = l.EnterAsync();

                tasks[i] = Task.Run(async () =>
                {
                    using (AsyncLockReleaser releaser = await releaserTask.ConfigureAwait(false))
                    {
                        Thread.Sleep(100);
                        ints.Add(_i);
                    }
                });
            }

            Task.WaitAll(tasks);

            Assert.True(ints.SequenceEqual(ints.OrderBy(i => i)));
        }
    }
}