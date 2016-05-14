#if NET_40

using System.Runtime.CompilerServices;

namespace System.Threading
{
    /// <summary>
    /// Slow Volatile reimplementation for
    /// consumers targeting .NET 4.0 and below.
    /// Equivalent to Thread.VolatileRead().
    /// </summary>
    internal static class Volatile
    {
        [MethodImplAttribute(MethodImplOptions.NoInlining)] // disable optimizations.
        public static T Read<T>(ref T location) where T : class
        {
            T value = location;
            Thread.MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
            return value;
        }
    }
}

#endif