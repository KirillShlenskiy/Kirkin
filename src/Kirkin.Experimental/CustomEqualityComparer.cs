using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kirkin
{
    /// <summary>
    /// IEqualityComparer implementation which uses a delegate.
    /// </summary>
    public sealed class CustomEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> __comparison;

        // Can be null.
        private readonly Func<T, int> __getHashCode;

        /// <summary>
        /// Returns true if the GetHashCode
        /// delegate was specified when this
        /// instance was created. If this value
        /// is false, calling GetHashCode on this
        /// instance will throw a NotSupportedException.
        /// </summary>
        public bool SupportsGetHashCode
        {
            get { return __getHashCode != null; }
        }

        /// <summary>
        /// Creates a new instance of CustomComparer.
        /// </summary>
        public CustomEqualityComparer(Func<T, T, bool> comparison)
            : this(comparison, null)
        {
        }

        /// <summary>
        /// Creates a new instance of CustomComparer.
        /// </summary>
        public CustomEqualityComparer(Func<T, T, bool> comparison, Func<T, int> getHashCode)
        {
            if (comparison == null) throw new ArgumentNullException("comparison");

            __comparison = comparison;
            __getHashCode = getHashCode; // Can be null.
        }

        /// <summary>
        /// Performs a comparison of two objects
        /// of the same type and returns a value
        /// indicating whether one object is less
        /// than, equal to, or greater than the other.
        /// </summary>
        public bool Equals(T x, T y)
        {
            return __comparison(x, y);
        }
        
        /// <summary>
        /// Returns the object's hashcode using
        /// the delegate specified when this
        /// instance was created, or throws
        /// if no such delegate was provided.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public int GetHashCode(T obj)
        {
            if (__getHashCode == null) {
                throw new NotSupportedException("GetHashCode delegate was not provided when this instance was created.");
            }

            return __getHashCode(obj);
        }
    }
}