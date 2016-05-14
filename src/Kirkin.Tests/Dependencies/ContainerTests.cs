using System;

using Kirkin.Dependencies;
using Kirkin.Functional;

using Xunit;

namespace Kirkin.Tests.Dependencies
{
    public class ContainerTests
    {
        [Fact]
        public void BasicResolution()
        {
            Container container = new Container();

            container.RegisterInstance(123);

            Assert.Equal(123, container.Resolve<int>());
            Assert.Equal(123, container.Resolve(typeof(int)));

            container.Deregister<int>();

            Assert.Throws<ResolutionFailedException>(() => container.Resolve<int>());

            int invokeCount = 0;

            container.RegisterFactory(() =>
            {
                invokeCount++;
                return 123;
            });

            Assert.Equal(123, container.Resolve<int>());
            Assert.Equal(123, container.Resolve(typeof(int)));
            Assert.Equal(2, invokeCount);

            invokeCount = 0;

            container.RegisterSingleton(() =>
            {
                invokeCount++;
                return 123;
            });

            Assert.Equal(123, container.Resolve<int>());
            Assert.Equal(123, container.Resolve(typeof(int)));
            Assert.Equal(1, invokeCount);

            Assert.Throws<ResolutionFailedException>(() => container.Resolve<IDisposable>());
            Assert.Throws<DisposableRegistrationException>(() => container.RegisterType<IDisposable, Dispo>());

            container.RegisterType<IDisposable, Dispo>(allowDisposable: true);

            Assert.NotNull(container.Resolve(typeof(IDisposable)) as Dispo);

            container.Dispose();

            Assert.Throws<ObjectDisposedException>(() => container.RegisterInstance("blah"));
            Assert.Throws<ObjectDisposedException>(() => container.Resolve<string>());
        }

        sealed class Dispo : IDisposable
        {
            public void Dispose()
            {
            }
        }

        [Fact]
        public void MultiResolve()
        {
            using (Container container = new Container())
            {
                container.RegisterInstance(123);
                container.RegisterFactory(c => new Dummy(c.Resolve<int>(), (string)c.Resolve(typeof(string))));

                Assert.Throws<ResolutionFailedException>(() => container.Resolve<Dummy>());

                container.RegisterFactory(() => "blah");

                Dummy dummy = container.Resolve<Dummy>();

                Assert.Equal(123, dummy.ID);
                Assert.Equal("blah", dummy.Value);

                container.Deregister(typeof(Dummy));

                Assert.Throws<ResolutionFailedException>(() => container.Resolve<Dummy>());

                container.RegisterFactory(() => new Dummy(container.Resolve<int>(), (string)container.Resolve(typeof(string))));

                dummy = container.Resolve<Dummy>();

                Assert.Equal(123, dummy.ID);
                Assert.Equal("blah", dummy.Value);
            }
        }

        [Fact]
        public void NestedScope()
        {
            bool disposed = false;
            IDisposable disposable = Disposable.Create(() => disposed = true);

            using (Container parent = new Container())
            {
                Assert.Throws<ResolutionFailedException>(() => parent.Resolve<IDisposable>());

                parent.RegisterInstance(disposable, dispose: false);

                using (Container child = parent.CreateChildContainer()) {
                    Assert.NotNull(child.Resolve<IDisposable>());
                }
            }

            Assert.False(disposed);

            using (Container parent = new Container())
            {
                Assert.Throws<ResolutionFailedException>(() => parent.Resolve<IDisposable>());

                parent.RegisterInstance(disposable, dispose: true);

                using (Container child = parent.CreateChildContainer()) {
                    Assert.NotNull(child.Resolve<IDisposable>());
                }

                Assert.False(disposed);
            }

            Assert.True(disposed);
        }

        [Fact]
        public void DisposeOnDeregistration()
        {
            bool disposed = false;
            IDisposable disposable = Disposable.Create(() => disposed = true);

            using (Container container = new Container())
            {
                Assert.Throws<ResolutionFailedException>(() => container.Resolve<IDisposable>());

                container.RegisterInstance(disposable, dispose: false);
                container.Deregister<IDisposable>();

                Assert.False(disposed);

                container.RegisterInstance(disposable, dispose: true);
                container.Deregister<IDisposable>();

                Assert.True(disposed);
            }
        }

        [Fact]
        public void DoNotResolveDerivedTypes()
        {
            using (Container container = new Container())
            {
                container.RegisterInstance(new DerivedDummy(123, "blah"));

                Assert.NotNull(container.Resolve<DerivedDummy>());
                Assert.Throws<ResolutionFailedException>(() => container.Resolve<Dummy>());

                container.Deregister<DerivedDummy>();

                Assert.Throws<ResolutionFailedException>(() => container.Resolve<DerivedDummy>());

                container.RegisterFactory(() => new DerivedDummy(123, "blah"));

                Assert.NotNull(container.Resolve<DerivedDummy>());
                Assert.Throws<ResolutionFailedException>(() => container.Resolve<Dummy>());
            }
        }

        class Dummy
        {
            public int ID { get; }
            public string Value { get; }

            public Dummy(int id, string value)
            {
                ID = id;
                Value = value;
            }
        }

        class DerivedDummy : Dummy
        {
            public DerivedDummy(int id, string value)
                : base(id, value)
            {
            }
        }
    }
}