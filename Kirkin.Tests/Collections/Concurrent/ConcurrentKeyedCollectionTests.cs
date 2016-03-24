using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Collections.Concurrent;
using Kirkin.Collections.Generic;

using Xunit;

namespace Kirkin.Tests.Collections.Concurrent
{
    public class ConcurrentKeyedCollectionTests
    {
        const int COUNT = 10000;

        // May not always fail.
        //[Fact]
        //public void KeyedCollectionFails()
        //{
        //    var collection = new KeyedCollection<int, int>(i => i);
        //    var tester = new KeyedCollectionTester<int, int>(collection);

        //    Assert.ThrowsAny<Exception>(() => RunParallelTests(tester, COUNT));
        //}

        [Fact]
        public void ConcurrentKeyedCollectionSucceeds()
        {
            var collection = new ConcurrentKeyedCollection<int, int>(i => i);
            var tester = new KeyedCollectionTester<int, int>(collection);

            RunParallelTests(tester, COUNT);
        }

        private void RunParallelTests(KeyedCollectionTester<int, int> tester, int count)
        {
            int i = 0;

            var adding = Task.Run(() =>
            {
                for (i = 0; i < count; i++) {
                    tester.Add(i);
                }
            });

            var removing = Task.Run(() =>
            {
                while (!adding.IsCompleted) {
                    tester.Remove(i);
                }
            });

            var clearing = Task.Run(() =>
            {
                while (!adding.IsCompleted) {
                    tester.Clear();
                }
            });

            var validation = Task.Run(() =>
            {
                while (!adding.IsCompleted) {
                    tester.Validate();
                }
            });

            Task.WaitAll(adding, removing, clearing, validation);

            tester.Validate();
        }

        sealed class KeyedCollectionTester<TKey, TItem> : IEnumerable<TItem>
        {
            private readonly KeyedCollectionBase<TKey, TItem> Collection;
            private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

            public KeyedCollectionTester(KeyedCollectionBase<TKey, TItem> collection)
            {
                Collection = collection;
            }

            public void Add(TItem item)
            {
                Lock.EnterReadLock();

                try
                {
                    Collection.Add(item);
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            public void Remove(TItem item)
            {
                Lock.EnterReadLock();

                try
                {
                    Collection.Remove(item);
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            public void Clear()
            {
                Lock.EnterReadLock();

                try
                {
                    Collection.Clear();
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            public IEnumerator<TItem> GetEnumerator()
            {
                Lock.EnterReadLock();

                try
                {
                    return Collection.AsEnumerable().GetEnumerator();
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Validate()
            {
                Lock.EnterWriteLock();

                try
                {
                    // Comparing cache to actual list of items.
                    Assert.True(Collection.SequenceEqual(Collection.Items.Values));
                }
                finally
                {
                    Lock.ExitWriteLock();
                }
            }
        }
    }
}