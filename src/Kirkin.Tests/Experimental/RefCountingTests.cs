using System;

using Kirkin.Functional;
using Kirkin.ReferenceCounting;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class RefCountingTests
    {
        [Test]
        public void BasicRefCounting()
        {
            bool created = false;
            bool disposed = false;

            SharedResourceManager<IDisposable> manager = new SharedResourceManager<IDisposable>(() =>
            {
                created = true;

                return Disposable.Create(() => disposed = true);
            });

            Assert.AreEqual(0, manager.ReferenceCount);
            Assert.False(created);
            Assert.False(disposed);

            Borrowed<IDisposable> ref1 = manager.Borrow();

            Assert.AreEqual(1, manager.ReferenceCount);
            Assert.False(created); // Lazy creation.
            Assert.False(disposed);

            ref1.Dispose();
            ref1.Dispose();

            Assert.AreEqual(0, manager.ReferenceCount);
            Assert.False(created);
            Assert.False(disposed);

            IDisposable ignored;

            Assert.Throws<ObjectDisposedException>(() => ignored = ref1.Value);

            ref1 = manager.Borrow();

            Assert.AreEqual(1, manager.ReferenceCount);

            IDisposable resource = ref1.Value;

            Assert.True(created);
            Assert.False(disposed);

            Borrowed<IDisposable> ref2 = manager.Borrow();

            Assert.AreEqual(2, manager.ReferenceCount);
            Assert.AreSame(ref1.Value, ref2.Value);
            Assert.True(created);
            Assert.False(disposed);

            ref1.Dispose();
            ref1.Dispose();

            Assert.AreEqual(1, manager.ReferenceCount);
            Assert.True(created);
            Assert.False(disposed);

            ref2.Dispose();
            ref2.Dispose();

            Assert.AreEqual(0, manager.ReferenceCount);
            Assert.True(created);
            Assert.True(disposed);
        }

        [Test]
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

            Assert.AreSame(resource1, resource2);

            using (Borrowed<IDisposable> ref2 = counter.Borrow()) {
                resource2 = ref2.Value;
            }

            Assert.AreNotSame(resource1, resource2);
        }

        [Test]
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