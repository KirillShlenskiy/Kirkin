using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Kirkin.Dependencies
{
    /// <summary>
    /// Simple thread-safe IoC container used for dependency injection.
    /// Performs registration and resolution of objects by their type.
    /// Does *not* perform any reflection-based resolution, constructor,
    /// property or field injection or weaving.
    /// </summary>
    public sealed class Container : IDisposable
    {
        // Note: in order for TryResolve<T> to work TValue must derive from ValueResolverBase<T>.
        // Set to null when the container is disposed.
        private ConcurrentDictionary<Type, IValueResolver> __valueResolvers = new ConcurrentDictionary<Type, IValueResolver>();
        private Container ParentContainer; // Only set on child containers.

        /// <summary>
        /// Gets the underlying value resolver mappings or throws if this instance has been disposed.
        /// </summary>
        private ConcurrentDictionary<Type, IValueResolver> ValueResolvers
        {
            get
            {
                // Intentionally non-volatile read.
                // We don't really care if we get a race here.
                ConcurrentDictionary<Type, IValueResolver> valueResolvers = __valueResolvers;

                if (valueResolvers == null) {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return valueResolvers;
            }
        }

        /// <summary>
        /// Creates a nested container scope.
        /// </summary>
        public Container CreateChildContainer()
        {
            return new Container { ParentContainer = this };
        }

        #region Registration

        /// <summary>
        /// Registers the given transient factory delegate which will
        /// be invoked every time an instance of type T is requested.
        /// Throws if T is IDisposable as its lifetime cannot be managed by this container.
        /// </summary>
        /// <param name="instanceFactory">Factory delegate which will be invoked to produce the transient instance.</param>
        public void RegisterFactory<T>(Func<T> instanceFactory)
        {
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            if (typeof(IDisposable).IsAssignableFrom(typeof(T))) throw new DisposableRegistrationException(typeof(T));

            ValueResolvers[typeof(T)] = new FactoryValueResolver<T>(instanceFactory);
        }

        /// <summary>
        /// Registers the given transient factory delegate which will
        /// be invoked every time an instance of type T is requested.
        /// </summary>
        /// <param name="instanceFactory">Factory delegate which will be invoked to produce the transient instance.</param>
        /// <param name="allowDisposable">
        /// When true, indicates that the caller is aware that the created IDisposable
        /// instance's lifetime will *not* be managed by this container.
        /// Suppresses the <see cref="DisposableRegistrationException"/>.
        /// </param>
        public void RegisterFactory<T>(Func<T> instanceFactory, bool allowDisposable)
            where T : IDisposable
        {
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            if (!allowDisposable && typeof(IDisposable).IsAssignableFrom(typeof(T))) throw new DisposableRegistrationException(typeof(T));

            ValueResolvers[typeof(T)] = new FactoryValueResolver<T>(instanceFactory);
        }

        /// <summary>
        /// Registers the given transient factory delegate which will
        /// be invoked every time an instance of type T is requested.
        /// Throws if T is IDisposable as its lifetime cannot be managed by this container.
        /// </summary>
        /// <param name="instanceFactory">Factory delegate which will be invoked to produce the transient instance.</param>
        public void RegisterFactory<T>(Func<Container, T> instanceFactory)
        {
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            if (typeof(IDisposable).IsAssignableFrom(typeof(T))) throw new DisposableRegistrationException(typeof(T));

            ValueResolvers[typeof(T)] = new ContainerFactoryValueResolver<T>(this, instanceFactory);
        }

        /// <summary>
        /// Registers the given transient factory delegate which will
        /// be invoked every time an instance of type T is requested.
        /// </summary>
        /// <param name="instanceFactory">Factory delegate which will be invoked to produce the transient instance.</param>
        /// <param name="allowDisposable">
        /// When true, indicates that the caller is aware that the created IDisposable
        /// instance's lifetime will *not* be managed by this container.
        /// Suppresses the <see cref="DisposableRegistrationException"/>.
        /// </param>
        public void RegisterFactory<T>(Func<Container, T> instanceFactory, bool allowDisposable)
            where T : IDisposable
        {
            if (instanceFactory == null) throw new ArgumentNullException(nameof(instanceFactory));
            if (!allowDisposable && typeof(IDisposable).IsAssignableFrom(typeof(T))) throw new DisposableRegistrationException(typeof(T));

            ValueResolvers[typeof(T)] = new ContainerFactoryValueResolver<T>(this, instanceFactory);
        }

        /// <summary>
        /// Registers a persistent instance which will be
        /// produced when an instance of type T is requested.
        /// Null is a valid value.
        /// </summary>
        public void RegisterInstance<T>(T instance)
        {
            ValueResolvers[typeof(T)] = new InstanceValueResolver<T>(instance);
        }

        /// <summary>
        /// Registers a persistent instance which will be
        /// produced when an instance of type T is requested.
        /// Null is a valid value.
        /// </summary>
        /// <param name="instance">Instance to be registered.</param>
        /// <param name="dispose">
        /// Signals that the instance should be disposed when the
        /// container is disposed or when the instance is deregistered.
        /// </param>
        public void RegisterInstance<T>(T instance, bool dispose)
            where T : IDisposable
        {
            ValueResolvers[typeof(T)] = dispose
                ? (IValueResolver)new DisposableInstanceValueResolver<T>(instance)
                : new InstanceValueResolver<T>(instance);
        }

        /// <summary>
        /// Registers the given factory delegate which will be
        /// invoked the first time an instance of type T is requested.
        /// This instance will be cached and reused for any subsequent requests.
        /// </summary>
        public void RegisterSingleton<T>(Func<T> instanceFactory)
        {
            ValueResolvers[typeof(T)] = new SingletonResolver<T>(instanceFactory);
        }

        /// <summary>
        /// Registers the given factory delegate which will be
        /// invoked the first time an instance of type T is requested.
        /// This instance will be cached and reused for any subsequent requests.
        /// </summary>
        public void RegisterSingleton<T>(Func<Container, T> instanceFactory)
        {
            ValueResolvers[typeof(T)] = new SingletonResolver<T>(() => instanceFactory(this));
        }

        /// <summary>
        /// Creates an association between the given type and
        /// its particular implementation. Any time an instance of
        /// type T is requested, a new transient instance of the
        /// implementing type will be constructed and returned.
        /// Throws if T is IDisposable as its lifetime cannot be managed by this container.
        /// </summary>
        public void RegisterType<T, TImplementation>()
            where TImplementation : T, new()
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T))) throw new DisposableRegistrationException(typeof(T));

            ValueResolvers[typeof(T)] = new DefaultConstructorResolver<T, TImplementation>();
        }

        /// <summary>
        /// Creates an association between the given type and
        /// its particular implementation. Any time an instance of
        /// type T is requested, a new transient instance of the
        /// implementing type will be constructed and returned.
        /// </summary>
        public void RegisterType<T, TImplementation>(bool allowDisposable)
            where T : IDisposable
            where TImplementation : T, new()
        {
            if (!allowDisposable && typeof(IDisposable).IsAssignableFrom(typeof(T))) throw new DisposableRegistrationException(typeof(T));

            ValueResolvers[typeof(T)] = new DefaultConstructorResolver<T, TImplementation>();
        }

        /// <summary>
        /// Deregisters resolution for the given type.
        /// </summary>
        public bool Deregister<T>()
        {
            return Deregister(typeof(T));
        }

        /// <summary>
        /// Deregisters resolution for the given type.
        /// </summary>
        public bool Deregister(Type type)
        {
            if (ValueResolvers.TryRemove(type, out IValueResolver valueResolver))
            {
                valueResolver.Dispose();
                return true;
            }

            return false;
        }

        #endregion

        #region Resolution

        /// <summary>
        /// Resolves an instance of the given type or throws if it cannot be resolved.
        /// </summary>
        public T Resolve<T>()
        {
            if (TryResolve(out T instance)) {
                return instance;
            }

            throw new ResolutionFailedException(typeof(T));
        }

        /// <summary>
        /// Resolves an instance of the given type or throws if it cannot be resolved.
        /// </summary>
        public object Resolve(Type type)
        {
            if (TryResolve(type, out object instance)) {
                return instance;
            }

            throw new ResolutionFailedException(type);
        }

        /// <summary>
        /// Resolves an instance of the given type. Returns the default value if it cannot be resolved.
        /// </summary>
        public T ResolveOrDefault<T>()
        {
            return ResolveOrDefault(default(T));
        }

        /// <summary>
        /// Resolves an instance of the given type. Returns the given value if it cannot be resolved.
        /// </summary>
        public T ResolveOrDefault<T>(T defaultValue)
        {
            return TryResolve(out T instance) ? instance : defaultValue;
        }

        /// <summary>
        /// Gets the instance of the given type, creating it if necessary.
        /// </summary>
        public bool TryResolve<T>(out T instance)
        {
            if (ValueResolvers.TryGetValue(typeof(T), out IValueResolver valueResolver))
            {
                instance = ((ValueResolverBase<T>)valueResolver).GetValue();
                return true;
            }

            if (ParentContainer != null) {
                return ParentContainer.TryResolve(out instance);
            }

            instance = default(T);
            return false;
        }

        /// <summary>
        /// Gets the instance of the given type, creating it if necessary.
        /// </summary>
        public bool TryResolve(Type type, out object instance)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (ValueResolvers.TryGetValue(type, out IValueResolver valueResolver))
            {
                instance = valueResolver.GetValue();
                return true;
            }

            if (ParentContainer != null) {
                return ParentContainer.TryResolve(type, out instance);
            }

            instance = null;
            return false;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Releases all resources used by this container.
        /// </summary>
        public void Dispose()
        {
            ConcurrentDictionary<Type, IValueResolver> valueResolvers = Interlocked.Exchange(ref __valueResolvers, null);

            if (valueResolvers != null)
            {
                foreach (KeyValuePair<Type, IValueResolver> kvp in valueResolvers) {
                    kvp.Value.Dispose();
                }
            }
        }

        #endregion

        #region Value resolvers

        interface IValueResolver : IDisposable
        {
            object GetValue();
        }

        abstract class ValueResolverBase<T> : IValueResolver
        {
            public abstract T GetValue();

            object IValueResolver.GetValue()
            {
                return GetValue();
            }

            // Called on Container.Dispose or item deregistration.
            public virtual void Dispose()
            {
            }
        }

        sealed class InstanceValueResolver<T> : ValueResolverBase<T>
        {
            private readonly T Instance;

            internal InstanceValueResolver(T instance)
            {
                Instance = instance;
            }

            public override T GetValue()
            {
                return Instance;
            }
        }

        sealed class DisposableInstanceValueResolver<T> : ValueResolverBase<T>
            where T : IDisposable
        {
            private readonly T Instance;

            internal DisposableInstanceValueResolver(T instance)
            {
                Instance = instance;
            }

            public override T GetValue()
            {
                return Instance;
            }

            public override void Dispose()
            {
                Instance?.Dispose();
            }
        }

        sealed class FactoryValueResolver<T> : ValueResolverBase<T>
        {
            private readonly Func<T> Factory;

            internal FactoryValueResolver(Func<T> factory)
            {
                Factory = factory;
            }

            public override T GetValue()
            {
                return Factory();
            }
        }

        sealed class ContainerFactoryValueResolver<T> : ValueResolverBase<T>
        {
            private readonly Container Container;
            private readonly Func<Container, T> Factory;

            internal ContainerFactoryValueResolver(Container container, Func<Container, T> factory)
            {
                Container = container;
                Factory = factory;
            }

            public override T GetValue()
            {
                return Factory(Container);
            }
        }

        sealed class SingletonResolver<T> : ValueResolverBase<T>
        {
            private Lazy<T> Lazy;

            internal SingletonResolver(Func<T> factory)
            {
                Lazy = new Lazy<T>(factory);
            }

            public override T GetValue()
            {
                return Lazy.Value;
            }

            public override void Dispose()
            {
                Lazy<T> lazy = Interlocked.Exchange(ref Lazy, null);

                if (lazy != null && lazy.IsValueCreated) {
                    (lazy.Value as IDisposable)?.Dispose();
                }
            }
        }

        sealed class DefaultConstructorResolver<T, TImplementation> : ValueResolverBase<T>
            where TImplementation : T, new()
        {
            public override T GetValue()
            {
                return new TImplementation();
            }
        }

        #endregion
    }
}