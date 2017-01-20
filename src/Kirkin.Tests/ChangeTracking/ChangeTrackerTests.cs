using System.Linq;

using Kirkin.ChangeTracking;
using Kirkin.Reflection;

using NUnit.Framework;

namespace Kirkin.Tests.ChangeTracking
{
    public class ChangeTrackerTests
    {
        [Test]
        public void DetectChanges()
        {
            Dummy dummy = new Dummy();
            ChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(dummy);

            Assert.False(tracker.DetectChanges().Any());

            dummy.ID = 1;

            var changes = tracker.DetectChanges().ToList();

            Assert.AreEqual(1, changes.Count);
            Assert.AreEqual(0, changes[0].OriginalValue);
            Assert.AreEqual(1, changes[0].CurrentValue);
        }

        [Test]
        public void Reset()
        {
            Dummy dummy = new Dummy();
            ChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(dummy);

            Assert.False(tracker.DetectChanges().Any());

            dummy.ID = 1;

            Assert.AreEqual(1, tracker.DetectChanges().Count());

            tracker.Reset();

            Assert.AreEqual(0, tracker.DetectChanges().Count());

            dummy.ID = 0;
            dummy.Value = "Zzz";

            Assert.AreEqual(2, tracker.DetectChanges().Count());
        }

        [Test]
        public void DetectChangesBenchmark()
        {
            Dummy dummy = new Dummy();
            ChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(dummy);

            for (int i = 0; i < 1000000; i++)
            {
                dummy.ID = i;

                tracker.DetectChanges().Count();
            }
        }

        [Test]
        public void ConstructorBenchmark()
        {
            Dummy dummy = new Dummy();

            for (int i = 0; i < 1000000; i++)
            {
                ChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(dummy);
            }
        }

        [Test]
        public void CustomPropertyList()
        {
            Dummy dummy = new Dummy();

            ChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(
                dummy, PropertyList<Dummy>.Default.Without(d => d.Value)
            );

            Assert.False(tracker.DetectChanges().Any());

            dummy.Value = "123";

            Assert.False(tracker.DetectChanges().Any());

            dummy.ID = 1;

            Assert.AreEqual(1, tracker.DetectChanges().Count());
            Assert.AreEqual("ID", tracker.DetectChanges().ElementAt(0).Property.Name);
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}