using System;

namespace Kirkin.Dependencies
{
    /// <summary>
    /// Thrown when a transient IDisposable is registered.
    /// </summary>
    [Serializable]
    public class DisposableRegistrationException : InvalidOperationException
    {
        /// <summary>
        /// Type which the caller was trying to register.
        /// </summary>
        public Type RegisteredType { get; }

        internal DisposableRegistrationException(Type registeredType)
            : base(
                  "Detected attempt to register a transient IDisposable implementation. " +
                  "The container will not be able to track its lifetime. Use the overload " +
                  "which supports the allowDisposable parameter to silence this exception.")
        {
            if (registeredType == null) throw new ArgumentNullException(nameof(registeredType));

            RegisteredType = registeredType;
        }
    }
}