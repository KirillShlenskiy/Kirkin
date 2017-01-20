using System;
using System.Diagnostics;

using Kirkin.Reflection;
using Kirkin.Transactions;

using NUnit.Framework;

namespace Kirkin.Tests.Transactions
{
    public class PropertyTrackingTransactionTests
    {
        [Test]
        public void Commit()
        {
            var dummy = new Dummy(1) { Value = "1" };

            using (var tran = new PropertyTrackingTransaction<Dummy>(dummy))
            {
                dummy.ID = 2;
                dummy.Value = "2";

                tran.Commit();
                Assert.AreSame(dummy, tran.ChangeTracker.TrackedObject);
            }

            Assert.AreEqual(2, dummy.ID);
            Assert.AreEqual("2", dummy.Value);
        }

        [Test]
        public void Rollback()
        {
            var dummy = new Dummy(1) { Value = "1" };

            using (var tran = new PropertyTrackingTransaction<Dummy>(dummy))
            {
                dummy.ID = 2;
                dummy.Value = "2";

                Assert.AreSame(dummy, tran.ChangeTracker.TrackedObject);
            }

            Assert.AreEqual(1, dummy.ID);
            Assert.AreEqual("1", dummy.Value);
        }

        [Test]
        public void RollbackPartial()
        {
            var dummy = new Dummy(1) { Value = "1" };

            using (var tran = new PropertyTrackingTransaction<Dummy>(dummy, PropertyList<Dummy>.Default.Without(d => d.Value)))
            {
                dummy.ID = 2;
                dummy.Value = "2";

                Assert.AreSame(dummy, tran.ChangeTracker.TrackedObject);
            }

            Assert.AreEqual(1, dummy.ID); // Rolled back.
            Assert.AreEqual("2", dummy.Value); // *NOT* rolled back.
        }

        [Test]
        public void EarlyCommit()
        {
            var dummy = new Dummy(1) { Value = "1" };

            using (var tran = new PropertyTrackingTransaction<Dummy>(dummy))
            {
                tran.Commit(); // Any changes from here on will be ignored.

                dummy.ID = 2;
                dummy.Value = "2";

                Assert.AreSame(dummy, tran.ChangeTracker.TrackedObject);
            }

            Assert.AreEqual(2, dummy.ID);
            Assert.AreEqual("2", dummy.Value);
        }

        [Test]
        public void ThrowsAfterDispose()
        {
            var dummy = new Dummy(1);
            var tran = new PropertyTrackingTransaction<Dummy>(dummy);

            tran.Dispose();

            Assert.Throws<ObjectDisposedException>(() => tran.Commit());

            // Ensure that multiple Dispose calls are fine.
            tran.Dispose();
            tran.Dispose();
        }

        [Test]
        public void ThrowsAfterCommitThenDispose()
        {
            var dummy = new Dummy(1);
            var tran = new PropertyTrackingTransaction<Dummy>(dummy);

            tran.Commit();
            tran.Dispose();

            Assert.Throws<ObjectDisposedException>(() => tran.Commit());

            // Ensure that multiple Dispose calls are fine.
            tran.Dispose();
            tran.Dispose();
        }

        [Test]
        public void ThrowsOnSecondCommit()
        {
            var dummy = new Dummy(1);

            using (var tran = new PropertyTrackingTransaction<Dummy>(dummy))
            {
                tran.Commit();
                Assert.Throws<InvalidOperationException>(() => tran.Commit());
            }
        }

        [Test]
        public void CommitBenchmark()
        {
            var dummy = new Dummy(1);

            for (int i = 0; i < 1000000; i++)
            {
                using (var tran = new PropertyTrackingTransaction<Dummy>(dummy))
                {
                    dummy.ID++;

                    tran.Commit();
                }
            }

            Assert.AreNotEqual(1, dummy.ID);
        }

        [Test]
        public void MemoryUsage()
        {
            long memStart = GC.GetTotalMemory(true);
            var arr = new PropertyTrackingTransaction[100000];
            var dummy = new Dummy(0);

            for (int i = 0; i < arr.Length; i++) {
                arr[i] = new PropertyTrackingTransaction<Dummy>(dummy);
            }

            long memEnd = GC.GetTotalMemory(true);
            long diff = memEnd - memStart;
            long intanceSize = diff / arr.Length;

            Debug.Print("Done.");
        }

        [Test]
        public void RollbackBenchmark()
        {
            var dummy = new Dummy(1);

            for (int i = 0; i < 1000000; i++)
            {
                using (var tran = new PropertyTrackingTransaction<Dummy>(dummy))
                {
                    dummy.ID++;

                    // No Commit call.
                }
            }

            Assert.AreEqual(1, dummy.ID);
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }

            public Dummy(int id) // Here to ensure that PropertyList.Map(Snapshot, T) is working with no new() constraint.
            {
                ID = id;
            }
        }
    }
}