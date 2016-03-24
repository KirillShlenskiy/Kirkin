using System;

namespace Kirkin.Dependencies
{
    /// <summary>
    /// Exception thrown when service resolution fails.
    /// </summary>
    [Serializable]
    public class ResolutionFailedException : InvalidOperationException
    {
        /// <summary>
        /// <see cref="Type"/> which could not be resolved.
        /// </summary>
        public Type UnresolvedType { get; }

        /// <summary>
        /// Creates a new <see cref="ResolutionFailedException"/> instance.
        /// </summary>
        internal ResolutionFailedException(Type unresolvedType)
            : base($"Instance of type {unresolvedType} could not be resolved.")
        {
            if (unresolvedType == null) throw new ArgumentNullException(nameof(unresolvedType));

            UnresolvedType = unresolvedType;
        }
    }
}