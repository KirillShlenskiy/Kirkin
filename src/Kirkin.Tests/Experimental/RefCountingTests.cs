using System;

using Kirkin.ReferenceCounting;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    public class RefCountingTests
    {
        [Fact]
        public void BasicRefCounting()
        {
            bool created = false;
            bool disposed = false;

            SharedResourceManager<IDisposable> manager = new SharedResourceManager<IDisposable>(() =>
            {
                created = true;

                return Disposable.Create(() => disposed = true);
            });

            Assert.Equal(0, manager.ReferenceCount);
            Assert.False(created);
            Assert.False(disposed);

            Borrowed<IDisposable> ref1 = manager.Borrow();

            Assert.Equal(1, manager.ReferenceCount);
            Assert.False(created); // Lazy creation.
            Assert.False(disposed);

            ref1.Dispose();
            ref1.Dispose();

            Assert.Equal(0, manager.ReferenceCount);
            Assert.False(created);
            Assert.False(disposed);
            Assert.Throws<ObjectDisposedException>(() => ref1.Value);

            ref1 = manager.Borrow();

            Assert.Equal(1, manager.ReferenceCount);

            IDisposable resource = ref1.Value;

            Assert.True(created);
            Assert.False(disposed);

            Borrowed<IDisposable> ref2 = manager.Borrow();

            Assert.Equal(2, manager.ReferenceCount);
            Assert.Same(ref1.Value, ref2.Value);
            Assert.True(created);
            Assert.False(disposed);

            ref1.Dispose();
            ref1.Dispose();

            Assert.Equal(1, manager.ReferenceCount);
            Assert.True(created);
            Assert.False(disposed);

            ref2.Dispose();
            ref2.Dispose();

            Assert.Equal(0, manager.ReferenceCount);
            Assert.True(created);
            Assert.True(disposed);
        }

        [Fact]
        public void AllowResurrect()
        {
            SharedResourceManager<IDisposable> counter = new SharedResourceManager<IDisposable>(
                () => Disposable.Create(() => { }),
                allowResurrect: true
            );

            IDisposable resource1;
            IDisposable resource2;

            using (Borrowed<IDisposable> ref1 = counter.Borrow())
            using (Borrowed<IDisposable> ref2 = counter.Borrow())
            {
                resource1 = ref1.Value;
                resource2 = ref2.Value;
            }

            Assert.Same(resource1, resource2);

            using (Borrowed<IDisposable> ref2 = counter.Borrow()) {
                resource2 = ref2.Value;
            }

            Assert.NotSame(resource1, resource2);
        }

        [Fact]
        public void DisallowResurrect()
        {
            SharedResourceManager<IDisposable> counter = new SharedResourceManager<IDisposable>(
                () => Disposable.Create(() => { }),
                allowResurrect: false
            );

            using (Borrowed<IDisposable> @ref = counter.Borrow()) {
                IDisposable resource = @ref.Value;
            }

            Assert.Throws<InvalidOperationException>(() => counter.Borrow());
        }
    }
}