using System;
using System.Collections.Generic;
using System.Linq;
namespace Kirkin.Security.Cryptography.Internal
{
    internal static class ArrayExtensions
    {
        public static ArraySegment<T> AsArraySegment<T>(this T[] array)
        {
            return new ArraySegment<T>(array);
        }
    }
}