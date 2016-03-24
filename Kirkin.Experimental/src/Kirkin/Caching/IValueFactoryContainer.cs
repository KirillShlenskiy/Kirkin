using System;

namespace Kirkin.Caching
{
    /// <summary>
    /// Interface implemented by types which define value factory members.
    /// </summary>
    /// <remarks>
    /// Primarily used for hiding value factory members from the public surface.
    /// </remarks>
    public interface IValueFactoryContainer<T>
    {
        /// <summary>
        /// Delegate invoked in order to generate the value.
        /// </summary>
        Func<T> ValueFactory { get; }
    }
}