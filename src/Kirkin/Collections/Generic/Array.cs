using System;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Common utility methods for working with generic arrays.
    /// </summary>
    public static class Array<T>
    {
        /// <summary>
        /// Shared empty array instance.
        /// </summary>
        public static readonly T[] Empty = new T[0];
    }
}