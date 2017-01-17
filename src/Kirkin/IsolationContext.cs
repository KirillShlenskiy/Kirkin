using System;
using System.Reflection;

namespace Kirkin
{
    /// <summary>
    /// Wraps and AppDomain and provides methods to execute code inside the domain.
    /// Useful when the object needs to access shared (static) state which should
    /// not leak to other similar objects.
    /// </summary>
    public sealed class IsolationContext
        : IDisposable
    {
        private AppDomain AppDomain; // Null if disposed.

        /// <summary>
        /// Creates a new instance of <see cref="IsolationContext"/>.
        /// </summary>
        public IsolationContext()
        {
            AppDomain = AppDomain.CreateDomain(
                $"Kirkin.{nameof(IsolationContext)}.{Guid.NewGuid()}", null, AppDomain.CurrentDomain.SetupInformation
            );
        }

        /// <summary>
        /// Creates a new instance of the given type inside the isolated domain.
        /// Type T must have a parameterless constructor.
        /// </summary>
        public T CreateInstance<T>()
            where T : MarshalByRefObject
        {
            ThrowIfDisposed();

            return (T)AppDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        /// <summary>
        /// Creates a new instance of the given type inside the isolated domain.
        /// Type T must have a constructor which matches the given arguments.
        /// </summary>
        public T CreateInstance<T>(params object[] args)
            where T : MarshalByRefObject
        {
            ThrowIfDisposed();

            return (T)AppDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, default(BindingFlags), null, args, null, null);
        }

        /// <summary>
        /// Unloads the domain.
        /// </summary>
        public void Dispose()
        {
            if (AppDomain != null)
            {
                AppDomain.Unload(AppDomain);

                AppDomain = null;
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if necessary.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (AppDomain == null) {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}