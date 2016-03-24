using System.Linq;

using Xunit;

using Kirkin.ChangeTracking;

namespace Kirkin.Tests.ChangeTracking
{
    public class ChangeTrackerTests
    {
        [Fact]
        public void DetectChanges()
        {
            Dummy dummy = new Dummy();
            IChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(dummy);

            Assert.False(tracker.DetectChanges().Any());

            dummy.ID = 1;

            var changes = tracker.DetectChanges().ToList();

            Assert.Equal(1, changes.Count);
            Assert.Equal(0, changes[0].OriginalValue);
            Assert.Equal(1, changes[0].CurrentValue);
        }

        [Fact]
        public void Reset()
        {
            Dummy dummy = new Dummy();
            ChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(dummy);

            Assert.False(tracker.DetectChanges().Any());

            dummy.ID = 1;

            Assert.Equal(1, tracker.DetectChanges().Count());

            tracker.Reset();

            Assert.Equal(0, tracker.DetectChanges().Count());

            dummy.ID = 0;
            dummy.Value = "Zzz";

            Assert.Equal(2, tracker.DetectChanges().Count());
        }

        [Fact]
        public void DetectChangesBenchmark()
        {
            Dummy dummy = new Dummy();
            IChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(dummy);

            for (int i = 0; i < 1000000; i++)
            {
                dummy.ID = i;

                tracker.DetectChanges().Count();
            }
        }

        [Fact]
        public void ConstructorBenchmark()
        {
            Dummy dummy = new Dummy();

            for (int i = 0; i < 1000000; i++)
            {
                IChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(dummy);
            }
        }

        [Fact]
        public void CustomTypeMapping()
        {
            Dummy dummy = new Dummy();

            IChangeTracker<Dummy> tracker = new ChangeTracker<Dummy>(
                dummy, TypeMapping<Dummy>.Default.Without(d => d.Value)
            );

            Assert.False(tracker.DetectChanges().Any());

            dummy.Value = "123";

            Assert.False(tracker.DetectChanges().Any());

            dummy.ID = 1;

            Assert.Equal(1, tracker.DetectChanges().Count());
            Assert.Equal("ID", tracker.DetectChanges().ElementAt(0).Property.Name);
        }

        private class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}