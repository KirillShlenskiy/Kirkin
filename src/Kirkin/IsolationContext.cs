﻿using System;
using System.Reflection;

namespace Kirkin
{
    /// <summary>
    /// Wraps an AppDomain and provides ways to execute code inside that AppDomain.
    /// Useful when an object needs to access shared (static) state which needs to
    /// be isolated from other, similar objects.
    /// </summary>
    public sealed class IsolationContext
        : /*MarshalByRefObject,*/ IDisposable
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

            //AppDomain.DomainUnload += AppDomainUnloadHandler;
        }

        ///// <summary>
        ///// Handles AppDomain unload.
        ///// </summary>
        //private void AppDomainUnloadHandler(object sender, EventArgs e)
        //{
        //    Dispose();
        //}

        /// <summary>
        /// Creates a new instance of the given type inside the isolated AppDomain.
        /// Type T must have a parameterless constructor.
        /// </summary>
        public T CreateInstance<T>()
            where T : MarshalByRefObject, new()
        {
            ThrowIfDisposed();

            return (T)AppDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        /// <summary>
        /// Creates a new instance of the given type inside the isolated AppDomain.
        /// Type T must have a constructor which matches the provided arguments.
        /// </summary>
        public T CreateInstance<T>(params object[] args)
            where T : MarshalByRefObject
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            ThrowIfDisposed();

            return (T)AppDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, false, default(BindingFlags), null, args, null, null);
        }

        /// <summary>
        /// Unloads the underlying AppDomain.
        /// </summary>
        public void Dispose()
        {
            if (AppDomain != null)
            {
                //AppDomain.DomainUnload -= AppDomainUnloadHandler;

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