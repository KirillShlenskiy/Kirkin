using System;

namespace Kirkin
{
    /// <summary>
    /// Represents an object instance which exists in its own isolated AppDomain.
    /// Useful when the object needs to access shared (static) state which should
    /// not leak to other similar objects.
    /// </summary>
    public sealed class IsolatedInstanceFactory
        : IDisposable
    {
        private AppDomain Domain; // Null if disposed.

        /// <summary>
        /// Creates a new instance of <see cref="IsolatedInstanceFactory"/>.
        /// </summary>
        public IsolatedInstanceFactory()
        {
            Domain = AppDomain.CreateDomain($"Kirkin.IsolatedDomain.{Guid.NewGuid()}", null, AppDomain.CurrentDomain.SetupInformation);
        }

        /// <summary>
        /// Creates a new instance of the given type inside the isolated domain.
        /// Type T must have a parameterless constructor.
        /// </summary>
        public T CreateInstance<T>()
            where T : MarshalByRefObject
        {
            if (Domain == null) {
                throw new ObjectDisposedException(GetType().Name);
            }

            return (T)Domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        /// <summary>
        /// Unloads the domain.
        /// </summary>
        public void Dispose()
        {
            if (Domain != null)
            {
                AppDomain.Unload(Domain);

                Domain = null;
            }
        }
    }
}